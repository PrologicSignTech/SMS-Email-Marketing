using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignContentDto
    {
        public ChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public List<string>? MediaUrls { get; set; }
        public int? TemplateId { get; set; }
        public Dictionary<string, string>? PersonalizationTokens { get; set; }
    }
}
