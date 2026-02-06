using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class Campaign : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecipients { get; set; } = 0;
        public int SuccessCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        
        // A/B Testing properties
        public bool IsABTest { get; set; } = false;
        public int? WinningVariantId { get; set; }
        public DateTime? ABTestEndDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual CampaignContent? Content { get; set; }
        public virtual CampaignAudience? Audience { get; set; }
        public virtual CampaignSchedule? Schedule { get; set; }
        public virtual ICollection<CampaignMessage> Messages { get; set; } = new List<CampaignMessage>();
        public virtual CampaignAnalytics? Analytics { get; set; }
        public virtual ICollection<URLShortener> URLShorteners { get; set; } = new List<URLShortener>();
        public virtual ICollection<CampaignVariant> Variants { get; set; } = new List<CampaignVariant>();
    }
}
