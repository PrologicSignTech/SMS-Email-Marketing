using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface ISuperAdminRepository : IRepository<SuperAdminRole>
    {
        /// <summary>
        /// Get all active super admin assignments
        /// </summary>
        Task<IEnumerable<SuperAdminRole>> GetActiveAsync();

        /// <summary>
        /// Get super admin assignment by user ID
        /// </summary>
        Task<SuperAdminRole?> GetByUserIdAsync(string userId);

        /// <summary>
        /// Assign super admin role to a user
        /// </summary>
        Task AssignAsync(SuperAdminRole superAdminRole);

        /// <summary>
        /// Revoke super admin role from a user
        /// </summary>
        Task<bool> RevokeAsync(string userId, string revokedBy, string reason);

        /// <summary>
        /// Check if a user is currently a super admin
        /// </summary>
        Task<bool> IsSuperAdminAsync(string userId);
    }
}
