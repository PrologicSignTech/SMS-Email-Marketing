using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents a platform-wide configuration setting
    /// </summary>
    public class PlatformSetting : BaseEntity
    {
        /// <summary>
        /// Unique key for the setting
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Setting value stored as string
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Data type of the value
        /// </summary>
        public SettingDataType DataType { get; set; } = SettingDataType.String;

        /// <summary>
        /// Scope of the setting (Global, Tenant, User)
        /// </summary>
        public SettingScope Scope { get; set; } = SettingScope.Global;

        /// <summary>
        /// Description of what this setting controls
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Category for grouping settings
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Whether this setting is encrypted
        /// </summary>
        public bool IsEncrypted { get; set; } = false;

        /// <summary>
        /// Whether this setting is read-only
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// Default value for the setting
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// User who last modified this setting
        /// </summary>
        public string? ModifiedBy { get; set; }
    }
}
