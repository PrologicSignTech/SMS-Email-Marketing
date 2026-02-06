using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignVariantDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TrafficPercentage { get; set; }
        public bool IsControl { get; set; }
        public bool IsActive { get; set; }
        
        public ChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? HTMLContent { get; set; }
        public string? MediaUrls { get; set; }
        public int? MessageTemplateId { get; set; }
        public string? PersonalizationTokens { get; set; }
        
        public int RecipientCount { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public CampaignVariantAnalyticsDto? Analytics { get; set; }
    }
}
