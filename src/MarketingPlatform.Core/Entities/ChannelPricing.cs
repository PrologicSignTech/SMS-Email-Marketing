using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ChannelPricing : BaseEntity
    {
        public int PricingModelId { get; set; }
        public ProviderType Channel { get; set; } // SMS, MMS, Email
        public decimal PricePerUnit { get; set; }
        public decimal? MinimumCharge { get; set; }
        public int? FreeUnitsIncluded { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Configuration { get; set; } // JSON for additional settings

        // Navigation properties
        public virtual PricingModel PricingModel { get; set; } = null!;
    }
}
