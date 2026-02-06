using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IPlatformConfigurationRepository : IRepository<PlatformConfiguration>
    {
        /// <summary>
        /// Get configuration by key
        /// </summary>
        Task<PlatformConfiguration?> GetByKeyAsync(string key);

        /// <summary>
        /// Get all configurations in a category
        /// </summary>
        Task<IEnumerable<PlatformConfiguration>> GetByCategoryAsync(ConfigurationCategory category);

        /// <summary>
        /// Update a configuration value
        /// </summary>
        Task<bool> UpdateConfigurationAsync(string key, string value, string modifiedBy);

        /// <summary>
        /// Get all active configurations
        /// </summary>
        Task<IEnumerable<PlatformConfiguration>> GetActiveConfigurationsAsync();
    }
}
