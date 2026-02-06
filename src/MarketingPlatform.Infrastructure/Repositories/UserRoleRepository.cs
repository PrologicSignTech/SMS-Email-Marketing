using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
        {
            return await _context.CustomUserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(int roleId)
        {
            return await _context.CustomUserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task<UserRole> AssignRoleToUserAsync(UserRole userRole)
        {
            _context.CustomUserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task RemoveRoleFromUserAsync(string userId, int roleId)
        {
            var userRole = await _context.CustomUserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            
            if (userRole != null)
            {
                _context.CustomUserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserHasRoleAsync(string userId, int roleId)
        {
            return await _context.CustomUserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<long> GetUserPermissionsAsync(string userId)
        {
            var userRoles = await _context.CustomUserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.Role.IsActive)
                .Select(ur => ur.Role.Permissions)
                .ToListAsync();

            // Combine all permissions using bitwise OR
            long combinedPermissions = 0;
            foreach (var permissions in userRoles)
            {
                combinedPermissions |= permissions;
            }

            return combinedPermissions;
        }
    }
}
