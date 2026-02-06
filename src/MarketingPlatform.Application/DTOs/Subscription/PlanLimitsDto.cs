namespace MarketingPlatform.Application.DTOs.Subscription
{
    public class PlanLimitsDto
    {
        public int SMSLimit { get; set; }
        public int MMSLimit { get; set; }
        public int EmailLimit { get; set; }
        public int ContactLimit { get; set; }
    }
}
