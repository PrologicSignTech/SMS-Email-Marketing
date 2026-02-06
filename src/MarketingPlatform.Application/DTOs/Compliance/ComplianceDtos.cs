using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Compliance
{
    public class ComplianceSettingsDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        // Double Opt-In Settings
        public bool RequireDoubleOptIn { get; set; }
        public bool RequireDoubleOptInSms { get; set; }
        public bool RequireDoubleOptInEmail { get; set; }
        
        // Quiet Hours Settings
        public bool EnableQuietHours { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public string? QuietHoursTimeZone { get; set; }
        
        // Company Information
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? PrivacyPolicyUrl { get; set; }
        public string? TermsOfServiceUrl { get; set; }
        
        // Auto-response Settings
        public string? OptOutKeywords { get; set; }
        public string? OptInKeywords { get; set; }
        public string? OptOutConfirmationMessage { get; set; }
        public string? OptInConfirmationMessage { get; set; }
        
        // Compliance Features
        public bool EnforceSuppressionList { get; set; }
        public bool EnableConsentTracking { get; set; }
        public bool EnableAuditLogging { get; set; }
        public int ConsentRetentionDays { get; set; }
    }

    public class UpdateComplianceSettingsDto
    {
        // Double Opt-In Settings
        public bool RequireDoubleOptIn { get; set; }
        public bool RequireDoubleOptInSms { get; set; }
        public bool RequireDoubleOptInEmail { get; set; }
        
        // Quiet Hours Settings
        public bool EnableQuietHours { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public string? QuietHoursTimeZone { get; set; }
        
        // Company Information
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? PrivacyPolicyUrl { get; set; }
        public string? TermsOfServiceUrl { get; set; }
        
        // Auto-response Settings
        public string? OptOutKeywords { get; set; }
        public string? OptInKeywords { get; set; }
        public string? OptOutConfirmationMessage { get; set; }
        public string? OptInConfirmationMessage { get; set; }
        
        // Compliance Features
        public bool EnforceSuppressionList { get; set; }
        public bool EnableConsentTracking { get; set; }
        public bool EnableAuditLogging { get; set; }
        public int ConsentRetentionDays { get; set; }
    }

    public class ComplianceAuditLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public int? CampaignId { get; set; }
        public ComplianceActionType ActionType { get; set; }
        public ConsentChannel Channel { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime ActionDate { get; set; }
        public string? ContactName { get; set; }
        public string? CampaignName { get; set; }
    }

    public class ComplianceCheckResultDto
    {
        public bool IsCompliant { get; set; }
        public List<string> Violations { get; set; } = new();
        public bool IsSuppressed { get; set; }
        public bool HasConsent { get; set; }
        public bool IsQuietHoursViolation { get; set; }
        public string? Message { get; set; }
    }

    public class QuietHoursCheckDto
    {
        public bool IsQuietHours { get; set; }
        public string? Message { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public DateTime? NextAllowedTime { get; set; }
    }
}
