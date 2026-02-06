namespace MarketingPlatform.Core.Enums
{
    /// <summary>
    /// Types of privileged actions that can be performed by super admins
    /// </summary>
    public enum PrivilegedActionType
    {
        // User & Role Management
        UserCreated,
        UserUpdated,
        UserDeleted,
        UserActivated,
        UserDeactivated,
        UserPasswordReset,
        RoleCreated,
        RoleUpdated,
        RoleDeleted,
        RolePermissionsModified,
        RoleAssigned,
        RoleRevoked,
        SuperAdminAssigned,
        SuperAdminRevoked,

        // Platform Configuration
        PlatformConfigurationUpdated,
        FeatureToggled,
        SystemSettingsModified,

        // Provider Management
        ProviderAdded,
        ProviderUpdated,
        ProviderRemoved,
        ProviderCredentialsUpdated,

        // Billing & Pricing
        PricingPlanCreated,
        PricingPlanUpdated,
        PricingPlanDeleted,
        SubscriptionModified,
        PaymentConfigurationUpdated,

        // Security & Compliance
        SecurityPolicyUpdated,
        ComplianceSettingsModified,
        AuditLogAccessed,
        EncryptionKeysRotated,
        DataRetentionPolicyUpdated,

        // System Operations
        DatabaseMigrationExecuted,
        SystemMaintenanceModeToggled,
        CacheCleared,
        BackupCreated,
        BackupRestored,

        // Abuse & Risk Management
        AccountSuspended,
        AccountReinstated,
        RateLimitOverridden,
        AbuseReportReviewed
    }

    /// <summary>
    /// Severity level of a privileged action
    /// </summary>
    public enum ActionSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Platform features that can be toggled
    /// </summary>
    public enum PlatformFeature
    {
        UserRegistration,
        EmailCampaigns,
        SMSCampaigns,
        ABTesting,
        Analytics,
        APIAccess,
        Integrations,
        AdvancedWorkflows,
        AudienceSegmentation,
        ComplianceTools,
        RateLimiting,
        FileStorage,
        ExternalAuth
    }

    /// <summary>
    /// Categories for platform configuration settings
    /// </summary>
    public enum ConfigurationCategory
    {
        General,
        Security,
        Messaging,
        Billing,
        Compliance,
        Performance,
        Integration,
        Features
    }
}
