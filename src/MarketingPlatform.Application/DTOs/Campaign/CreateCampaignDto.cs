using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CreateCampaignDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignContentDto Content { get; set; } = new();
        public CampaignAudienceDto Audience { get; set; } = new();
        public CampaignScheduleDto? Schedule { get; set; }
    }
}
