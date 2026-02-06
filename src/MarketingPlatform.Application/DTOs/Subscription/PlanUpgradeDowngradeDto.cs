namespace MarketingPlatform.Application.DTOs.Subscription
{
    public class PlanUpgradeDowngradeDto
    {
        public int CurrentPlanId { get; set; }
        public int NewPlanId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool Prorated { get; set; } = true;
    }
}
