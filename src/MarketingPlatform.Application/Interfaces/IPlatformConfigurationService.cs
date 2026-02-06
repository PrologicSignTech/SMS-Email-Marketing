using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPlatformConfigurationService
    {
        /// <summary>
        /// Get configuration by key
        /// </summary>
        Task<PlatformConfigurationDto?> GetByKeyAsync(string key);

        /// <summary>
        /// Get all configurations in a category
        /// </summary>
        Task<IEnumerable<PlatformConfigurationDto>> GetByCategoryAsync(ConfigurationCategory category);

        /// <summary>
        /// Get all active configurations
        /// </summary>
        Task<IEnumerable<PlatformConfigurationDto>> GetActiveConfigurationsAsync();

        /// <summary>
        /// Update a configuration value
        /// </summary>
        Task<bool> UpdateConfigurationAsync(string modifiedBy, UpdateConfigurationDto request);

        /// <summary>
        /// Toggle a platform feature
        /// </summary>
        Task<bool> ToggleFeatureAsync(string modifiedBy, PlatformFeature feature, bool enabled);
    }
}
