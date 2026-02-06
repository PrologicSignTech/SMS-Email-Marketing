using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordDto
    {
        public int Id { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsGloballyReserved { get; set; }
        public KeywordStatus Status { get; set; }
        public string? ResponseMessage { get; set; }
        public int? LinkedCampaignId { get; set; }
        public int? OptInGroupId { get; set; }
        public string? LinkedCampaignName { get; set; }
        public string? OptInGroupName { get; set; }
        public int ActivityCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
