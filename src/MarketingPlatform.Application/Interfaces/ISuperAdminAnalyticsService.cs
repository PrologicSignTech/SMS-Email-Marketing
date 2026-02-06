using MarketingPlatform.Application.DTOs.Analytics;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISuperAdminAnalyticsService
    {
        // Platform Analytics
        Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync();
        Task<List<UserGrowthDto>> GetUserGrowthAsync(int months);
        Task<List<SubscriptionsByPlanDto>> GetSubscriptionDistributionAsync();
        
        // Billing Analytics
        Task<BillingAnalyticsDto> GetBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> CalculateChurnRateAsync(int months = 1);
        Task<decimal> CalculateARPUAsync();
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months);
        
        // System Health Monitoring
        Task<SystemHealthDto> GetSystemHealthAsync();
        Task<List<ProviderHealthDto>> GetProviderHealthAsync();
        Task<MessageDeliveryStatsDto> GetMessageDeliveryStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
