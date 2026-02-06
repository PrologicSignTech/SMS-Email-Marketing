using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class CampaignAudience : BaseEntity
    {
        public int CampaignId { get; set; }
        public TargetType TargetType { get; set; }
        public string? GroupIds { get; set; } // JSON array
        public string? SegmentCriteria { get; set; } // JSON
        public string? ExclusionListIds { get; set; } // JSON array

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}
