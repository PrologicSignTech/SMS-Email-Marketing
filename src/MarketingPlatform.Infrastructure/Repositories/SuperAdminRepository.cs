using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class SuperAdminRepository : Repository<SuperAdminRole>, ISuperAdminRepository
    {
        public SuperAdminRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SuperAdminRole>> GetActiveAsync()
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.AssignedByUser)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.AssignedAt)
                .ToListAsync();
        }

        public async Task<SuperAdminRole?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.AssignedByUser)
                .Include(s => s.RevokedByUser)
                .Where(s => s.UserId == userId && s.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task AssignAsync(SuperAdminRole superAdminRole)
        {
            // Revoke any existing active assignments first
            var existing = await _dbSet
                .Where(s => s.UserId == superAdminRole.UserId && s.IsActive)
                .ToListAsync();

            foreach (var item in existing)
            {
                item.IsActive = false;
                item.RevokedAt = DateTime.UtcNow;
                // Use the person performing the new assignment as the revoker for audit clarity
                item.RevokedBy = superAdminRole.AssignedBy;
                item.RevocationReason = "Automatically revoked due to new super admin assignment";
            }

            await AddAsync(superAdminRole);
        }

        public async Task<bool> RevokeAsync(string userId, string revokedBy, string reason)
        {
            var superAdminRole = await _dbSet
                .Where(s => s.UserId == userId && s.IsActive)
                .FirstOrDefaultAsync();

            if (superAdminRole == null)
                return false;

            superAdminRole.IsActive = false;
            superAdminRole.RevokedAt = DateTime.UtcNow;
            superAdminRole.RevokedBy = revokedBy;
            superAdminRole.RevocationReason = reason;

            Update(superAdminRole);
            return true;
        }

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            return await _dbSet
                .AnyAsync(s => s.UserId == userId && s.IsActive);
        }
    }
}
