namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class DashboardSummaryDto
    {
        // Campaign Overview
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedCampaigns { get; set; }
        public int ScheduledCampaigns { get; set; }

        // Message Statistics
        public int TotalMessagesSent { get; set; }
        public int TotalMessagesDelivered { get; set; }
        public int TotalMessagesFailed { get; set; }
        public decimal OverallDeliveryRate { get; set; }

        // Engagement Metrics
        public int TotalClicks { get; set; }
        public decimal OverallClickRate { get; set; }
        public int TotalOptOuts { get; set; }
        public decimal OverallOptOutRate { get; set; }

        // Contact Statistics
        public int TotalContacts { get; set; }
        public int ActiveContacts { get; set; }
        public int EngagedContacts { get; set; }

        // Financial Summary
        public decimal TotalSpent { get; set; }
        public decimal AverageCostPerMessage { get; set; }

        // Recent Activity
        public List<CampaignPerformanceDto> RecentCampaigns { get; set; } = new();
        public List<TopPerformerDto> TopPerformingCampaigns { get; set; } = new();
    }

    public class TopPerformerDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public decimal PerformanceScore { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal ClickRate { get; set; }
    }
}
