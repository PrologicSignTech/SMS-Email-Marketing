namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class BillingAnalyticsDto
    {
        public decimal MRR { get; set; }
        public decimal ARR { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal ARPU { get; set; }
        public decimal AverageLTV { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<ChurnByPlanDto> ChurnByPlan { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int NewSubscriptions { get; set; }
        public int CanceledSubscriptions { get; set; }
    }

    public class ChurnByPlanDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int TotalSubscribers { get; set; }
        public int Churned { get; set; }
        public decimal ChurnRate { get; set; }
    }
}
