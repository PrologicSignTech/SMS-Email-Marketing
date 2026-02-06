namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class PlatformAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialUsers { get; set; }
        public int CanceledSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MRR { get; set; }
        public decimal ARPU { get; set; }
        public decimal ChurnRate { get; set; }
        public List<SubscriptionsByPlanDto> SubscriptionsByPlan { get; set; } = new();
        public List<UsageByChannelDto> UsageByChannel { get; set; } = new();
    }

    public class SubscriptionsByPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UsageByChannelDto
    {
        public string Channel { get; set; } = string.Empty;
        public int TotalUsage { get; set; }
        public decimal AveragePerUser { get; set; }
    }
}
