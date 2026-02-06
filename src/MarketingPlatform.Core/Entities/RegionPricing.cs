namespace MarketingPlatform.Core.Entities
{
    public class RegionPricing : BaseEntity
    {
        public int PricingModelId { get; set; }
        public string RegionCode { get; set; } = string.Empty; // Country code or region identifier
        public string RegionName { get; set; } = string.Empty;
        public decimal PriceMultiplier { get; set; } = 1.0m; // Multiplier applied to base price
        public decimal? FlatAdjustment { get; set; } // Fixed amount added/subtracted
        public bool IsActive { get; set; } = true;
        public string? Configuration { get; set; } // JSON for additional settings

        // Navigation properties
        public virtual PricingModel PricingModel { get; set; } = null!;
    }
}
