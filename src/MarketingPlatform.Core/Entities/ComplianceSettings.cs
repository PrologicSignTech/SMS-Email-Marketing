namespace MarketingPlatform.Core.Entities
{
    public class ComplianceSettings : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        
        // Double Opt-In Settings
        public bool RequireDoubleOptIn { get; set; } = false;
        public bool RequireDoubleOptInSms { get; set; } = false;
        public bool RequireDoubleOptInEmail { get; set; } = false;
        
        // Quiet Hours Settings
        public bool EnableQuietHours { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public string? QuietHoursTimeZone { get; set; } // IANA timezone
        
        // Company Information
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? PrivacyPolicyUrl { get; set; }
        public string? TermsOfServiceUrl { get; set; }
        
        // Auto-response Settings
        public string? OptOutKeywords { get; set; } // Comma-separated (STOP, UNSUBSCRIBE, etc.)
        public string? OptInKeywords { get; set; } // Comma-separated (START, SUBSCRIBE, etc.)
        public string? OptOutConfirmationMessage { get; set; }
        public string? OptInConfirmationMessage { get; set; }
        
        // Compliance Features
        public bool EnforceSuppressionList { get; set; } = true;
        public bool EnableConsentTracking { get; set; } = true;
        public bool EnableAuditLogging { get; set; } = true;
        public int ConsentRetentionDays { get; set; } = 2555; // ~7 years default

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
