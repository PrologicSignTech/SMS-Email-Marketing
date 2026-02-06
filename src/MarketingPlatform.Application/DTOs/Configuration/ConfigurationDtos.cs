using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Configuration
{
    public class PlatformSettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public SettingDataType DataType { get; set; }
        public SettingScope Scope { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsReadOnly { get; set; }
        public string? DefaultValue { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePlatformSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public SettingDataType DataType { get; set; } = SettingDataType.String;
        public SettingScope Scope { get; set; } = SettingScope.Global;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsEncrypted { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public string? DefaultValue { get; set; }
    }

    public class UpdatePlatformSettingDto
    {
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
    }

    public class FeatureToggleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FeatureToggleStatus Status { get; set; }
        public bool IsEnabled { get; set; }
        public string? EnabledForRoles { get; set; }
        public string? EnabledForUsers { get; set; }
        public string? Category { get; set; }
        public DateTime? EnableAfter { get; set; }
        public DateTime? DisableAfter { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateFeatureToggleDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = false;
        public string? Category { get; set; }
        public DateTime? EnableAfter { get; set; }
        public DateTime? DisableAfter { get; set; }
    }

    public class UpdateFeatureToggleDto
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public bool? IsEnabled { get; set; }
        public string? Category { get; set; }
        public DateTime? EnableAfter { get; set; }
        public DateTime? DisableAfter { get; set; }
    }

    public class ToggleFeatureDto
    {
        public bool IsEnabled { get; set; }
        public string? EnabledForRoles { get; set; }
        public string? EnabledForUsers { get; set; }
    }

    public class ComplianceRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplianceRuleType RuleType { get; set; }
        public ComplianceRuleStatus Status { get; set; }
        public string Configuration { get; set; } = "{}";
        public int Priority { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ApplicableRegions { get; set; }
        public string? ApplicableServices { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateComplianceRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplianceRuleType RuleType { get; set; }
        public string Configuration { get; set; } = "{}";
        public int Priority { get; set; } = 0;
        public bool IsMandatory { get; set; } = true;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ApplicableRegions { get; set; }
        public string? ApplicableServices { get; set; }
    }

    public class UpdateComplianceRuleDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Configuration { get; set; }
        public int? Priority { get; set; }
        public bool? IsMandatory { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ApplicableRegions { get; set; }
        public string? ApplicableServices { get; set; }
        public string? Reason { get; set; }
    }

    public class ComplianceRuleAuditDto
    {
        public int Id { get; set; }
        public int ComplianceRuleId { get; set; }
        public ComplianceAuditAction Action { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string? PreviousState { get; set; }
        public string? NewState { get; set; }
        public string? Reason { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
