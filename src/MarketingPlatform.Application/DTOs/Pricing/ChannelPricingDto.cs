using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Pricing
{
    public class ChannelPricingDto
    {
        public int Id { get; set; }
        public int PricingModelId { get; set; }
        public string? PricingModelName { get; set; }
        public ProviderType Channel { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal? MinimumCharge { get; set; }
        public int? FreeUnitsIncluded { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChannelPricingDto
    {
        public int PricingModelId { get; set; }
        public ProviderType Channel { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal? MinimumCharge { get; set; }
        public int? FreeUnitsIncluded { get; set; }
        public string? Configuration { get; set; }
    }

    public class UpdateChannelPricingDto
    {
        public ProviderType Channel { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal? MinimumCharge { get; set; }
        public int? FreeUnitsIncluded { get; set; }
        public bool IsActive { get; set; }
        public string? Configuration { get; set; }
    }
}
