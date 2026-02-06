using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPlatformSettingService
    {
        Task<PlatformSettingDto?> GetSettingByIdAsync(int id);
        Task<PlatformSettingDto?> GetSettingByKeyAsync(string key);
        Task<PaginatedResult<PlatformSettingDto>> GetSettingsAsync(PagedRequest request);
        Task<List<PlatformSettingDto>> GetSettingsByCategoryAsync(string category);
        Task<PlatformSettingDto> CreateSettingAsync(CreatePlatformSettingDto dto, string userId);
        Task<bool> UpdateSettingAsync(int id, UpdatePlatformSettingDto dto, string userId);
        Task<bool> DeleteSettingAsync(int id);
        Task<T?> GetSettingValueAsync<T>(string key, T? defaultValue = default);
        Task<bool> SetSettingValueAsync(string key, object value, string userId);
    }

    public interface IFeatureToggleService
    {
        Task<FeatureToggleDto?> GetFeatureToggleByIdAsync(int id);
        Task<FeatureToggleDto?> GetFeatureToggleByNameAsync(string name);
        Task<PaginatedResult<FeatureToggleDto>> GetFeatureTogglesAsync(PagedRequest request);
        Task<List<FeatureToggleDto>> GetFeatureTogglesByCategoryAsync(string category);
        Task<FeatureToggleDto> CreateFeatureToggleAsync(CreateFeatureToggleDto dto, string userId);
        Task<bool> UpdateFeatureToggleAsync(int id, UpdateFeatureToggleDto dto, string userId);
        Task<bool> DeleteFeatureToggleAsync(int id);
        Task<bool> ToggleFeatureAsync(int id, ToggleFeatureDto dto, string userId);
        Task<bool> IsFeatureEnabledAsync(string featureName);
        Task<bool> IsFeatureEnabledForUserAsync(string featureName, string userId);
        Task<bool> IsFeatureEnabledForRoleAsync(string featureName, string roleName);
    }

    public interface IComplianceRuleService
    {
        Task<ComplianceRuleDto?> GetComplianceRuleByIdAsync(int id);
        Task<PaginatedResult<ComplianceRuleDto>> GetComplianceRulesAsync(PagedRequest request);
        Task<List<ComplianceRuleDto>> GetActiveComplianceRulesAsync();
        Task<List<ComplianceRuleDto>> GetComplianceRulesByTypeAsync(Core.Enums.ComplianceRuleType ruleType);
        Task<ComplianceRuleDto> CreateComplianceRuleAsync(CreateComplianceRuleDto dto, string userId);
        Task<bool> UpdateComplianceRuleAsync(int id, UpdateComplianceRuleDto dto, string userId, string? ipAddress = null);
        Task<bool> DeleteComplianceRuleAsync(int id, string userId, string? reason = null, string? ipAddress = null);
        Task<bool> ActivateComplianceRuleAsync(int id, string userId, string? ipAddress = null);
        Task<bool> DeactivateComplianceRuleAsync(int id, string userId, string? reason = null, string? ipAddress = null);
        Task<List<ComplianceRuleAuditDto>> GetComplianceRuleAuditTrailAsync(int ruleId);
        Task<List<ComplianceRuleDto>> GetApplicableRulesAsync(string? region = null, string? service = null);
    }
}
