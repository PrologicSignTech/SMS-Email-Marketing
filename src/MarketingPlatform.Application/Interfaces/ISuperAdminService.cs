using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISuperAdminService
    {
        /// <summary>
        /// Get all active super admin assignments
        /// </summary>
        Task<IEnumerable<SuperAdminRoleDto>> GetActiveSuperAdminsAsync();

        /// <summary>
        /// Get super admin assignment by user ID
        /// </summary>
        Task<SuperAdminRoleDto?> GetSuperAdminByUserIdAsync(string userId);

        /// <summary>
        /// Assign super admin role to a user
        /// </summary>
        Task<SuperAdminRoleDto> AssignSuperAdminAsync(string assignedBy, AssignSuperAdminDto request);

        /// <summary>
        /// Revoke super admin role from a user
        /// </summary>
        Task<bool> RevokeSuperAdminAsync(string revokedBy, RevokeSuperAdminDto request);

        /// <summary>
        /// Check if a user is currently a super admin
        /// </summary>
        Task<bool> IsSuperAdminAsync(string userId);
    }
}
