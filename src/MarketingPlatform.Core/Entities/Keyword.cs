using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class Keyword : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string KeywordText { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsGloballyReserved { get; set; } = false;
        public KeywordStatus Status { get; set; } = KeywordStatus.Active;
        public string? ResponseMessage { get; set; }
        public int? LinkedCampaignId { get; set; }
        public int? OptInGroupId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Campaign? LinkedCampaign { get; set; }
        public virtual ContactGroup? OptInGroup { get; set; }
        public virtual ICollection<KeywordActivity> Activities { get; set; } = new List<KeywordActivity>();
    }
}
