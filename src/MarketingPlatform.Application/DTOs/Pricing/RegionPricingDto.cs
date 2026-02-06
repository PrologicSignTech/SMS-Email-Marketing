namespace MarketingPlatform.Application.DTOs.Pricing
{
    public class RegionPricingDto
    {
        public int Id { get; set; }
        public int PricingModelId { get; set; }
        public string? PricingModelName { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public decimal PriceMultiplier { get; set; }
        public decimal? FlatAdjustment { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRegionPricingDto
    {
        public int PricingModelId { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public decimal PriceMultiplier { get; set; } = 1.0m;
        public decimal? FlatAdjustment { get; set; }
        public string? Configuration { get; set; }
    }

    public class UpdateRegionPricingDto
    {
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public decimal PriceMultiplier { get; set; }
        public decimal? FlatAdjustment { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
    }
}
