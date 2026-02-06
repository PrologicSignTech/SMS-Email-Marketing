using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class UsagePricing : BaseEntity
    {
        public int PricingModelId { get; set; }
        public UsagePricingType Type { get; set; }
        public int TierStart { get; set; } // Starting usage count for this tier
        public int? TierEnd { get; set; } // Ending usage count (null for unlimited)
        public decimal PricePerUnit { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Configuration { get; set; } // JSON for additional settings

        // Navigation properties
        public virtual PricingModel PricingModel { get; set; } = null!;
    }
}
