using MarketingPlatform.Application.DTOs.Analytics;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IAnalyticsService
    {
        // Dashboard
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId, ReportFilterDto? filter = null);

        // Campaign Performance
        Task<List<CampaignPerformanceDto>> GetCampaignPerformanceAsync(string userId, ReportFilterDto filter);
        Task<CampaignPerformanceDto?> GetCampaignPerformanceByIdAsync(string userId, int campaignId);

        // Contact Engagement
        Task<List<ContactEngagementHistoryDto>> GetContactEngagementHistoryAsync(string userId, ReportFilterDto filter);
        Task<ContactEngagementHistoryDto?> GetContactEngagementByIdAsync(string userId, int contactId, ReportFilterDto? filter = null);

        // Conversion Tracking
        Task<ConversionTrackingDto?> GetConversionTrackingAsync(string userId, int campaignId, ReportFilterDto? filter = null);
        Task<List<ConversionTrackingDto>> GetConversionTrackingForCampaignsAsync(string userId, ReportFilterDto filter);
    }
}
