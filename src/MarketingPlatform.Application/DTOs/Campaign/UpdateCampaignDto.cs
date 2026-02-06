using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class UpdateCampaignDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignContentDto? Content { get; set; }
        public CampaignAudienceDto? Audience { get; set; }
        public CampaignScheduleDto? Schedule { get; set; }
    }
}
