using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents a feature toggle for dynamic feature management
    /// </summary>
    public class FeatureToggle : BaseEntity
    {
        /// <summary>
        /// Unique name for the feature
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the feature
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the feature
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Current status of the feature toggle
        /// </summary>
        public FeatureToggleStatus Status { get; set; } = FeatureToggleStatus.Disabled;

        /// <summary>
        /// Whether feature is enabled (simple on/off)
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Comma-separated list of role names that can access this feature
        /// </summary>
        public string? EnabledForRoles { get; set; }

        /// <summary>
        /// Comma-separated list of user IDs that can access this feature
        /// </summary>
        public string? EnabledForUsers { get; set; }

        /// <summary>
        /// Feature category for organization
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// When the feature should automatically enable
        /// </summary>
        public DateTime? EnableAfter { get; set; }

        /// <summary>
        /// When the feature should automatically disable
        /// </summary>
        public DateTime? DisableAfter { get; set; }

        /// <summary>
        /// User who last modified this toggle
        /// </summary>
        public string? ModifiedBy { get; set; }
    }
}
