using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Platform-wide configuration settings with change tracking
    /// </summary>
    public class PlatformConfiguration : BaseEntity
    {
        /// <summary>
        /// Unique configuration key
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Configuration value (stored as string, parsed based on DataType)
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Category of this configuration setting
        /// </summary>
        public ConfigurationCategory Category { get; set; }

        /// <summary>
        /// Data type of the value (e.g., "string", "int", "bool", "json")
        /// </summary>
        public string DataType { get; set; } = "string";

        /// <summary>
        /// Human-readable description of this configuration
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether the value is encrypted in the database
        /// </summary>
        public bool IsEncrypted { get; set; } = false;

        /// <summary>
        /// Whether this configuration is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User ID of the person who last modified this configuration
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// When this configuration was last modified
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser? LastModifiedByUser { get; set; }
    }
}
