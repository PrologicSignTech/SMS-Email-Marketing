using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class CreateKeywordDto
    {
        public string KeywordText { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ResponseMessage { get; set; }
        public int? LinkedCampaignId { get; set; }
        public int? OptInGroupId { get; set; }
    }
}
