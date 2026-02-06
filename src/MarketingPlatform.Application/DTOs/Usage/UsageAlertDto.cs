namespace MarketingPlatform.Application.DTOs.Usage
{
    public class UsageAlertDto
    {
        public int UsagePercentage { get; set; }
        public string Channel { get; set; } = string.Empty;
        public int Used { get; set; }
        public int Limit { get; set; }
        public DateTime AlertTime { get; set; }
    }
}
