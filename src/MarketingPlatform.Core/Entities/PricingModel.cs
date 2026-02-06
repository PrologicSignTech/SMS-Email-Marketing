using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class PricingModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PricingModelType Type { get; set; }
        public decimal BasePrice { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Configuration { get; set; } // JSON for model-specific settings
        public int Priority { get; set; } = 0; // Higher priority models are evaluated first

        // Navigation properties
        public virtual ICollection<ChannelPricing> ChannelPricings { get; set; } = new List<ChannelPricing>();
        public virtual ICollection<RegionPricing> RegionPricings { get; set; } = new List<RegionPricing>();
        public virtual ICollection<UsagePricing> UsagePricings { get; set; } = new List<UsagePricing>();
    }
}
