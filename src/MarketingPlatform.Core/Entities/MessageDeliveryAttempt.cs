using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class MessageDeliveryAttempt : BaseEntity
    {
        public int CampaignMessageId { get; set; }
        public int AttemptNumber { get; set; }
        public ChannelType Channel { get; set; }
        public string? ProviderName { get; set; }
        public DateTime AttemptedAt { get; set; }
        public bool Success { get; set; }
        public string? ExternalMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public decimal? CostAmount { get; set; }
        public int ResponseTimeMs { get; set; }
        public FallbackReason? FallbackReason { get; set; }
        public string? AdditionalMetadata { get; set; } // JSON for extra info

        // Navigation properties
        public virtual CampaignMessage CampaignMessage { get; set; } = null!;
    }
}
