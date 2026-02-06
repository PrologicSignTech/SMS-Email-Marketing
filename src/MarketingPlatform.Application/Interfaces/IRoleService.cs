using MarketingPlatform.Application.DTOs.Role;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto?> GetRoleByNameAsync(string name);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<IEnumerable<RoleDto>> GetActiveRolesAsync();
        Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto, string createdBy);
        Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto updateRoleDto);
        Task DeleteRoleAsync(int id);
        Task<IEnumerable<UserRoleDto>> GetUsersInRoleAsync(int roleId);
        Task AssignRoleToUserAsync(string userId, int roleId, string assignedBy);
        Task RemoveRoleFromUserAsync(string userId, int roleId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, Permission permission);
    }
}
