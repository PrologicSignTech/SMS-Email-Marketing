using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(int roleId);
        Task<UserRole> AssignRoleToUserAsync(UserRole userRole);
        Task RemoveRoleFromUserAsync(string userId, int roleId);
        Task<bool> UserHasRoleAsync(string userId, int roleId);
        Task<long> GetUserPermissionsAsync(string userId);
    }
}
