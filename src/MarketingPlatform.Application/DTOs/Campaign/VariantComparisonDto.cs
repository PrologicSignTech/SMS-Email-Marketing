namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class VariantComparisonDto
    {
        public List<CampaignVariantDto> Variants { get; set; } = new();
        public CampaignVariantDto? WinningVariant { get; set; }
        public string? RecommendedAction { get; set; }
    }
}
