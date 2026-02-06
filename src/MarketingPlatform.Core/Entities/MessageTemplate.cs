using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class MessageTemplate : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ChannelType Channel { get; set; }
        public TemplateCategory Category { get; set; }
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public string? DefaultMediaUrls { get; set; } // JSON array
        public string? TemplateVariables { get; set; } // JSON array of variables
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<CampaignContent> CampaignContents { get; set; } = new List<CampaignContent>();
    }
}
