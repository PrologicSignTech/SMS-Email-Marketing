namespace MarketingPlatform.Application.DTOs.Billing
{
    public class SubscriptionUpgradeDto
    {
        public int NewPlanId { get; set; }
        public bool Prorated { get; set; } = true;
    }
}
