using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignAudienceDto
    {
        public TargetType TargetType { get; set; }
        public List<int>? GroupIds { get; set; }
        public string? SegmentCriteria { get; set; }
        public List<int>? ExclusionListIds { get; set; }
    }
}
