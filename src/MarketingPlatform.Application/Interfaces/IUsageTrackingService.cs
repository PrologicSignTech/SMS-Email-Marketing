using MarketingPlatform.Application.DTOs.Usage;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IUsageTrackingService
    {
        // Usage Aggregation
        Task<UsageStatsDto> GetCurrentUsageAsync(string userId);
        Task<UsageStatsDto> GetUsageForPeriodAsync(string userId, DateTime startDate, DateTime endDate);
        Task<bool> TrackUsageAsync(string userId, string channel, int count);
        Task<bool> ResetMonthlyUsageAsync(string userId);
        
        // Overage Calculation
        Task<decimal> CalculateOverageAsync(string userId);
        Task<OverageDetailsDto> GetOverageDetailsAsync(string userId);
        
        // Usage Alerts
        Task<List<UsageAlertDto>> GetUsageAlertsAsync(string userId);
        Task<bool> CheckAndCreateAlertsAsync(string userId);
        Task<bool> SetAlertThresholdAsync(string userId, string channel, int percentage);
        
        // Revenue Analytics
        Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetMRRAsync();
        Task<decimal> GetARRAsync();
        Task<List<RevenueByPlanDto>> GetRevenueByPlanAsync(DateTime startDate, DateTime endDate);
    }
}
