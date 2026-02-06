using AutoMapper;
using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ISuperAdminRepository _superAdminRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(
            ISuperAdminRepository superAdminRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SuperAdminService> logger)
        {
            _superAdminRepository = superAdminRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SuperAdminRoleDto>> GetActiveSuperAdminsAsync()
        {
            var superAdmins = await _superAdminRepository.GetActiveAsync();
            return _mapper.Map<IEnumerable<SuperAdminRoleDto>>(superAdmins);
        }

        public async Task<SuperAdminRoleDto?> GetSuperAdminByUserIdAsync(string userId)
        {
            var superAdmin = await _superAdminRepository.GetByUserIdAsync(userId);
            return superAdmin == null ? null : _mapper.Map<SuperAdminRoleDto>(superAdmin);
        }

        public async Task<SuperAdminRoleDto> AssignSuperAdminAsync(string assignedBy, AssignSuperAdminDto request)
        {
            _logger.LogInformation("Assigning super admin role to user {UserId} by {AssignedBy}", request.UserId, assignedBy);

            var superAdminRole = new SuperAdminRole
            {
                UserId = request.UserId,
                AssignedBy = assignedBy,
                AssignmentReason = request.AssignmentReason,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _superAdminRepository.AssignAsync(superAdminRole);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Super admin role assigned successfully to user {UserId}", request.UserId);

            return _mapper.Map<SuperAdminRoleDto>(superAdminRole);
        }

        public async Task<bool> RevokeSuperAdminAsync(string revokedBy, RevokeSuperAdminDto request)
        {
            _logger.LogInformation("Revoking super admin role from user {UserId} by {RevokedBy}", request.UserId, revokedBy);

            var result = await _superAdminRepository.RevokeAsync(request.UserId, revokedBy, request.RevocationReason);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Super admin role revoked successfully from user {UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("Failed to revoke super admin role from user {UserId} - no active assignment found", request.UserId);
            }

            return result;
        }

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            return await _superAdminRepository.IsSuperAdminAsync(userId);
        }
    }
}
