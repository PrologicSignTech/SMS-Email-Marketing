using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Compliance;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IComplianceService
    {
        // Consent Management
        Task<ConsentStatusDto?> GetContactConsentStatusAsync(string userId, int contactId);
        Task<ContactConsentDto> RecordConsentAsync(string userId, ConsentRequestDto request);
        Task<int> BulkRecordConsentAsync(string userId, BulkConsentRequestDto request);
        Task<bool> RevokeConsentAsync(string userId, int contactId, ConsentChannel channel, string? reason = null);
        Task<PaginatedResult<ContactConsentDto>> GetContactConsentsAsync(string userId, int contactId, PagedRequest request);
        Task<PaginatedResult<ConsentHistoryDto>> GetConsentHistoryAsync(string userId, int contactId, PagedRequest request);

        // Compliance Settings
        Task<ComplianceSettingsDto> GetComplianceSettingsAsync(string userId);
        Task<ComplianceSettingsDto> UpdateComplianceSettingsAsync(string userId, UpdateComplianceSettingsDto dto);
        Task<ComplianceSettingsDto> CreateDefaultComplianceSettingsAsync(string userId);

        // Compliance Checks
        Task<ComplianceCheckResultDto> CheckComplianceAsync(string userId, int contactId, ConsentChannel channel, int? campaignId = null);
        Task<QuietHoursCheckDto> CheckQuietHoursAsync(string userId, DateTime? sendTime = null);
        Task<bool> IsContactSuppressedAsync(string userId, string phoneOrEmail, ConsentChannel? channel = null);
        Task<List<int>> FilterCompliantContactsAsync(string userId, List<int> contactIds, ConsentChannel channel, int? campaignId = null);

        // Audit Logging
        Task LogComplianceActionAsync(string userId, ComplianceActionType actionType, ConsentChannel channel, 
            int? contactId = null, int? campaignId = null, string? description = null, string? ipAddress = null);
        Task<PaginatedResult<ComplianceAuditLogDto>> GetAuditLogsAsync(string userId, PagedRequest request, 
            ComplianceActionType? actionType = null, ConsentChannel? channel = null, int? contactId = null);
        
        // Opt-In/Opt-Out Automation
        Task<bool> ProcessOptOutKeywordAsync(string userId, int contactId, string keyword, ConsentChannel channel);
        Task<bool> ProcessOptInKeywordAsync(string userId, int contactId, string keyword, ConsentChannel channel);
        Task<string?> GetOptOutConfirmationMessageAsync(string userId);
        Task<string?> GetOptInConfirmationMessageAsync(string userId);
    }
}
