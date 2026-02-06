namespace MarketingPlatform.Application.DTOs.Usage
{
    public class OverageDetailsDto
    {
        public int SMSOverage { get; set; }
        public int MMSOverage { get; set; }
        public int EmailOverage { get; set; }
        public decimal SMSOverageCost { get; set; }
        public decimal MMSOverageCost { get; set; }
        public decimal EmailOverageCost { get; set; }
        public decimal TotalOverageCost { get; set; }
    }
}
