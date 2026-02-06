namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class SystemHealthDto
    {
        public bool IsHealthy { get; set; }
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int MessagesSentToday { get; set; }
        public int FailedMessagesToday { get; set; }
        public decimal MessageSuccessRate { get; set; }
        public List<ProviderHealthDto> ProviderHealth { get; set; } = new();
        public DateTime LastChecked { get; set; }
    }

    public class ProviderHealthDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime? LastSuccess { get; set; }
        public DateTime? LastFailure { get; set; }
    }

    public class MessageDeliveryStatsDto
    {
        public int TotalSent { get; set; }
        public int Delivered { get; set; }
        public int Failed { get; set; }
        public decimal DeliveryRate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class UserGrowthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
    }
}
