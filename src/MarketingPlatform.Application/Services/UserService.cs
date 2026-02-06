using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.User;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;

namespace MarketingPlatform.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await MapToUserDtoAsync(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            return await MapToUserDtoAsync(user);
        }

        public async Task<PaginatedResult<UserDto>> GetUsersAsync(PagedRequest request)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email!.Contains(request.SearchTerm) ||
                    u.FirstName!.Contains(request.SearchTerm) ||
                    u.LastName!.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(await MapToUserDtoAsync(user));
            }

            return new PaginatedResult<UserDto>
            {
                Items = userDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(string userId)
        {
            var campaignRepo = _unitOfWork.Repository<Campaign>();
            var contactRepo = _unitOfWork.Repository<Contact>();
            var messageRepo = _unitOfWork.Repository<CampaignMessage>();
            var subscriptionRepo = _unitOfWork.Repository<UserSubscription>();

            var totalCampaigns = await campaignRepo.CountAsync(c => c.UserId == userId);
            var totalContacts = await contactRepo.CountAsync(c => c.UserId == userId);
            var totalMessages = await messageRepo.CountAsync(m => m.Campaign.UserId == userId);
            var activeCampaigns = await campaignRepo.CountAsync(c => c.UserId == userId && c.Status == CampaignStatus.Running);

            var subscription = await subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);
            var planName = "Free";
            
            if (subscription != null)
            {
                var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(subscription.SubscriptionPlanId);
                planName = plan?.Name ?? "Free";
            }

            return new UserStatsDto
            {
                TotalCampaigns = totalCampaigns,
                TotalContacts = totalContacts,
                TotalMessagesSent = totalMessages,
                ActiveCampaigns = activeCampaigns,
                TotalSpent = 0, // TODO: Implement billing calculation - track invoice totals when billing system is added
                SubscriptionPlan = planName
            };
        }
    }
}
