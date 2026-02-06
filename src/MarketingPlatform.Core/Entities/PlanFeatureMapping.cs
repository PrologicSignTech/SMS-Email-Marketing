namespace MarketingPlatform.Core.Entities
{
    public class PlanFeatureMapping : BaseEntity
    {
        public int SubscriptionPlanId { get; set; }
        public int FeatureId { get; set; }
        public bool IsIncluded { get; set; } = true;
        public string? FeatureValue { get; set; } // For features with specific values (e.g., "10,000 contacts")
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public virtual Feature Feature { get; set; } = null!;
    }
}
