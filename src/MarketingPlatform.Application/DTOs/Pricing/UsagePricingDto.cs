using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Pricing
{
    public class UsagePricingDto
    {
        public int Id { get; set; }
        public int PricingModelId { get; set; }
        public string? PricingModelName { get; set; }
        public UsagePricingType Type { get; set; }
        public int TierStart { get; set; }
        public int? TierEnd { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUsagePricingDto
    {
        public int PricingModelId { get; set; }
        public UsagePricingType Type { get; set; }
        public int TierStart { get; set; }
        public int? TierEnd { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? Configuration { get; set; }
    }

    public class UpdateUsagePricingDto
    {
        public UsagePricingType Type { get; set; }
        public int TierStart { get; set; }
        public int? TierEnd { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
    }
}
