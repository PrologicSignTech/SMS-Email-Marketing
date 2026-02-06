namespace MarketingPlatform.Application.DTOs.Usage
{
    public class UsageStatsDto
    {
        public string UserId { get; set; } = string.Empty;
        public int SMSCount { get; set; }
        public int MMSCount { get; set; }
        public int EmailCount { get; set; }
        public int SMSLimit { get; set; }
        public int MMSLimit { get; set; }
        public int EmailLimit { get; set; }
        public decimal SMSOverage { get; set; }
        public decimal MMSOverage { get; set; }
        public decimal EmailOverage { get; set; }
        public decimal OverageCost { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
