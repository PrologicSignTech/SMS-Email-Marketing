namespace MarketingPlatform.Application.DTOs.Usage
{
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal SubscriptionRevenue { get; set; }
        public decimal OverageRevenue { get; set; }
        public decimal MRR { get; set; }
        public decimal ARR { get; set; }
        public List<RevenueByPlanDto> RevenueByPlan { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class RevenueByPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int SubscriberCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
