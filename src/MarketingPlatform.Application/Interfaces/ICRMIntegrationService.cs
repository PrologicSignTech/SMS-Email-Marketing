using MarketingPlatform.Application.DTOs.Integration;

namespace MarketingPlatform.Application.Interfaces
{
    /// <summary>
    /// Interface for CRM system integration
    /// </summary>
    public interface ICRMIntegrationService
    {
        /// <summary>
        /// Syncs contacts from CRM to the platform
        /// </summary>
        Task<CRMSyncResultDto> SyncContactsFromCRMAsync(string userId, CRMSyncConfigDto config);

        /// <summary>
        /// Syncs contacts from platform to CRM
        /// </summary>
        Task<CRMSyncResultDto> SyncContactsToCRMAsync(string userId, List<int> contactIds, CRMSyncConfigDto config);

        /// <summary>
        /// Syncs campaign results to CRM
        /// </summary>
        Task<bool> SyncCampaignResultsToCRMAsync(string userId, int campaignId, CRMSyncConfigDto config);

        /// <summary>
        /// Tests CRM connection with provided credentials
        /// </summary>
        Task<CRMConnectionTestResultDto> TestConnectionAsync(CRMSyncConfigDto config);

        /// <summary>
        /// Gets available CRM fields for mapping
        /// </summary>
        Task<List<CRMFieldDto>> GetCRMFieldsAsync(CRMSyncConfigDto config);
    }
}
