using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class CampaignMessage : BaseEntity
    {
        public int CampaignId { get; set; }
        public int ContactId { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public ChannelType Channel { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Queued;
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? HTMLContent { get; set; }
        public string? MediaUrls { get; set; } // JSON array
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? ExternalMessageId { get; set; }
        public string? ProviderMessageId { get; set; } // Keep for backward compatibility
        public string? ErrorMessage { get; set; }
        public decimal CostAmount { get; set; } = 0.00m;
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        
        // A/B Testing
        public int? VariantId { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual Contact Contact { get; set; } = null!;
        public virtual CampaignVariant? Variant { get; set; }
    }
}
