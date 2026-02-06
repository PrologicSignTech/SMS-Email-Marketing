using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class SecurityBadgeService : ISecurityBadgeService
    {
        private readonly IRepository<SecurityBadge> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SecurityBadgeService> _logger;

        public SecurityBadgeService(
            IRepository<SecurityBadge> repository,
            IUnitOfWork unitOfWork,
            ILogger<SecurityBadgeService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<SecurityBadge>> GetAllActiveAsync()
        {
            var badges = await _repository.FindAsync(b => 
                b.IsActive && b.ShowOnLanding && !b.IsDeleted);
            return badges.OrderBy(b => b.DisplayOrder);
        }

        public async Task<SecurityBadge?> GetByIdAsync(int id)
        {
            var badge = await _repository.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            return badge;
        }

        public async Task<SecurityBadge> CreateAsync(SecurityBadge badge)
        {
            badge.CreatedAt = DateTime.UtcNow;
            badge.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(badge);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created security badge: {Title}", badge.Title);

            return badge;
        }

        public async Task<SecurityBadge> UpdateAsync(int id, SecurityBadge badge)
        {
            var existingBadge = await _repository.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            
            if (existingBadge == null)
                throw new InvalidOperationException("Security badge not found");

            existingBadge.Title = badge.Title;
            existingBadge.Subtitle = badge.Subtitle;
            existingBadge.IconUrl = badge.IconUrl;
            existingBadge.Description = badge.Description;
            existingBadge.DisplayOrder = badge.DisplayOrder;
            existingBadge.IsActive = badge.IsActive;
            existingBadge.ShowOnLanding = badge.ShowOnLanding;
            existingBadge.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existingBadge);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated security badge: {Title}", existingBadge.Title);

            return existingBadge;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var badge = await _repository.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            
            if (badge == null)
                throw new InvalidOperationException("Security badge not found");

            badge.IsDeleted = true;
            badge.UpdatedAt = DateTime.UtcNow;

            _repository.Update(badge);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted security badge: {Title}", badge.Title);

            return true;
        }
    }
}
