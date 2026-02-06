namespace MarketingPlatform.Core.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PlanCategory { get; set; } // e.g., "For small businesses", "For growing teams"
        public bool IsMostPopular { get; set; } = false;
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int SMSLimit { get; set; }
        public int MMSLimit { get; set; }
        public int EmailLimit { get; set; }
        public int ContactLimit { get; set; }
        public string? Features { get; set; } // JSON - Deprecated, use PlanFeatureMappings instead
        
        // Stripe Integration
        public string? StripeProductId { get; set; }
        public string? StripePriceIdMonthly { get; set; }
        public string? StripePriceIdYearly { get; set; }
        
        // PayPal Integration
        public string? PayPalProductId { get; set; }
        public string? PayPalPlanIdMonthly { get; set; }
        public string? PayPalPlanIdYearly { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool ShowOnLanding { get; set; } = true; // Control whether plan appears on landing page

        // Navigation properties
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
        public virtual ICollection<PlanFeatureMapping> PlanFeatureMappings { get; set; } = new List<PlanFeatureMapping>();
    }
}
