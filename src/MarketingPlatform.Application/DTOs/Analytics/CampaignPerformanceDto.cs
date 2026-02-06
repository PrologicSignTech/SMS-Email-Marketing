namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class CampaignPerformanceDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Message Statistics
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public int TotalClicks { get; set; }
        public int TotalOptOuts { get; set; }

        // Performance Metrics
        public decimal DeliveryRate { get; set; }
        public decimal ClickRate { get; set; }
        public decimal OptOutRate { get; set; }
        public decimal EngagementRate { get; set; }

        // Financial Metrics
        public decimal EstimatedCost { get; set; }
        public decimal CostPerMessage { get; set; }
        public decimal CostPerClick { get; set; }

        // Time Metrics
        public TimeSpan? Duration { get; set; }
        public double? AverageDeliveryTime { get; set; }
    }
}
