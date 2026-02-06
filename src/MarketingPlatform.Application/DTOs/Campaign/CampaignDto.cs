using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsABTest { get; set; }
        public int? WinningVariantId { get; set; }
        public DateTime? ABTestEndDate { get; set; }
        public CampaignContentDto? Content { get; set; }
        public CampaignAudienceDto? Audience { get; set; }
        public CampaignScheduleDto? Schedule { get; set; }
        public List<CampaignVariantDto>? Variants { get; set; }
    }
}
