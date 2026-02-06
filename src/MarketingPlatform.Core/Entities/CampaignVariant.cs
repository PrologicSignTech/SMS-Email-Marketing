using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class CampaignVariant : BaseEntity
    {
        public int CampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TrafficPercentage { get; set; } = 0;
        public bool IsControl { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        // Variant-specific content
        public ChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? HTMLContent { get; set; }
        public string? MediaUrls { get; set; } // JSON array
        public int? MessageTemplateId { get; set; }
        public string? PersonalizationTokens { get; set; } // JSON dictionary
        
        // Statistics
        public int RecipientCount { get; set; } = 0;
        public int SentCount { get; set; } = 0;
        public int DeliveredCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;
        
        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual MessageTemplate? MessageTemplate { get; set; }
        public virtual CampaignVariantAnalytics? Analytics { get; set; }
        public virtual ICollection<CampaignMessage> Messages { get; set; } = new List<CampaignMessage>();
    }
}
