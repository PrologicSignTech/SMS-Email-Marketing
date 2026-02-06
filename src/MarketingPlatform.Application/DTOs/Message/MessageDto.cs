using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int ContactId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public ChannelType Channel { get; set; }
        public MessageStatus Status { get; set; }
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public List<string>? MediaUrls { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ExternalMessageId { get; set; }
        public decimal? CostAmount { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
