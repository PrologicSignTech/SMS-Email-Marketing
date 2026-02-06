using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Pricing
{
    public class PricingModelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PricingModelType Type { get; set; }
        public decimal BasePrice { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ChannelPricingDto> ChannelPricings { get; set; } = new();
        public List<RegionPricingDto> RegionPricings { get; set; } = new();
        public List<UsagePricingDto> UsagePricings { get; set; } = new();
    }

    public class CreatePricingModelDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PricingModelType Type { get; set; }
        public decimal BasePrice { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public string? Configuration { get; set; }
        public int Priority { get; set; } = 0;
    }

    public class UpdatePricingModelDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PricingModelType Type { get; set; }
        public decimal BasePrice { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
        public int Priority { get; set; }
    }
}
