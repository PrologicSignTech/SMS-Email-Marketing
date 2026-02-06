using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class PlatformConfigurationRepository : Repository<PlatformConfiguration>, IPlatformConfigurationRepository
    {
        public PlatformConfigurationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PlatformConfiguration?> GetByKeyAsync(string key)
        {
            return await _dbSet
                .Include(p => p.LastModifiedByUser)
                .FirstOrDefaultAsync(p => p.Key == key);
        }

        public async Task<IEnumerable<PlatformConfiguration>> GetByCategoryAsync(ConfigurationCategory category)
        {
            return await _dbSet
                .Include(p => p.LastModifiedByUser)
                .Where(p => p.Category == category)
                .OrderBy(p => p.Key)
                .ToListAsync();
        }

        public async Task<bool> UpdateConfigurationAsync(string key, string value, string modifiedBy)
        {
            var config = await _dbSet.FirstOrDefaultAsync(p => p.Key == key);

            if (config == null)
                return false;

            config.Value = value;
            config.LastModifiedBy = modifiedBy;
            config.LastModifiedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;

            Update(config);
            return true;
        }

        public async Task<IEnumerable<PlatformConfiguration>> GetActiveConfigurationsAsync()
        {
            return await _dbSet
                .Include(p => p.LastModifiedByUser)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Key)
                .ToListAsync();
        }
    }
}
