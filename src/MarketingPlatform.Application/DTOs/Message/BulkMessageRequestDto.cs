using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    public class BulkMessageRequestDto
    {
        public int CampaignId { get; set; }
        public List<int> ContactIds { get; set; } = new();
        public ChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public List<string>? MediaUrls { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
