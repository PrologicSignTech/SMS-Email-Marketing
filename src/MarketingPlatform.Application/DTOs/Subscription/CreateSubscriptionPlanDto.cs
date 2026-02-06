namespace MarketingPlatform.Application.DTOs.Subscription
{
    public class CreateSubscriptionPlanDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int SMSLimit { get; set; }
        public int MMSLimit { get; set; }
        public int EmailLimit { get; set; }
        public int ContactLimit { get; set; }
        public Dictionary<string, object>? Features { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool ShowOnLanding { get; set; } = true;
        public bool CreateStripeProduct { get; set; } = true;
        public bool CreatePayPalProduct { get; set; } = false;
    }
}
