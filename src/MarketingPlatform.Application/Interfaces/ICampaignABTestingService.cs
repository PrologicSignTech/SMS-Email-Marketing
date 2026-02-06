using MarketingPlatform.Application.DTOs.Campaign;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ICampaignABTestingService
    {
        Task<CampaignVariantDto> CreateVariantAsync(string userId, int campaignId, CreateCampaignVariantDto dto);
        Task<CampaignVariantDto?> GetVariantByIdAsync(string userId, int campaignId, int variantId);
        Task<List<CampaignVariantDto>> GetCampaignVariantsAsync(string userId, int campaignId);
        Task<bool> UpdateVariantAsync(string userId, int campaignId, int variantId, UpdateCampaignVariantDto dto);
        Task<bool> DeleteVariantAsync(string userId, int campaignId, int variantId);
        Task<bool> ActivateVariantAsync(string userId, int campaignId, int variantId);
        Task<bool> DeactivateVariantAsync(string userId, int campaignId, int variantId);
        Task<VariantComparisonDto> CompareVariantsAsync(string userId, int campaignId);
        Task<bool> SelectWinningVariantAsync(string userId, int campaignId, int variantId);
        Task<CampaignVariantDto?> SelectVariantForRecipientAsync(int campaignId);
        Task UpdateVariantAnalyticsAsync(int variantId);
        Task<bool> ValidateVariantTrafficAllocationAsync(int campaignId);
    }
}
