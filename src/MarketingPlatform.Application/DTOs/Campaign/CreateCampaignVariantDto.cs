using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CreateCampaignVariantDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TrafficPercentage { get; set; }
        public bool IsControl { get; set; } = false;
        
        public ChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? HTMLContent { get; set; }
        public string? MediaUrls { get; set; }
        public int? MessageTemplateId { get; set; }
        public string? PersonalizationTokens { get; set; }
    }
}
