namespace MarketingPlatform.Core.Enums
{
    /// <summary>
    /// Defines the scope of a platform setting
    /// </summary>
    public enum SettingScope
    {
        Global = 0,
        Tenant = 1,
        User = 2
    }

    /// <summary>
    /// Defines the data type of a setting value
    /// </summary>
    public enum SettingDataType
    {
        String = 0,
        Integer = 1,
        Boolean = 2,
        Decimal = 3,
        Json = 4
    }

    /// <summary>
    /// Feature toggle status
    /// </summary>
    public enum FeatureToggleStatus
    {
        Disabled = 0,
        Enabled = 1,
        EnabledForRoles = 2,
        EnabledForUsers = 3
    }

    /// <summary>
    /// Compliance rule type
    /// </summary>
    public enum ComplianceRuleType
    {
        DataRetention = 0,
        ConsentManagement = 1,
        MessageContent = 2,
        RateLimiting = 3,
        OptOutEnforcement = 4,
        RegionalCompliance = 5,
        Custom = 99
    }

    /// <summary>
    /// Compliance rule status
    /// </summary>
    public enum ComplianceRuleStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Archived = 3
    }

    /// <summary>
    /// Compliance rule audit action
    /// </summary>
    public enum ComplianceAuditAction
    {
        Created = 0,
        Updated = 1,
        Activated = 2,
        Deactivated = 3,
        Deleted = 4
    }
}
