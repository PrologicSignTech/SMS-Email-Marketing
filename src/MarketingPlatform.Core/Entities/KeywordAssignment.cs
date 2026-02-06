namespace MarketingPlatform.Core.Entities
{
    public class KeywordAssignment : BaseEntity
    {
        public int KeywordId { get; set; }
        public int CampaignId { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UnassignedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Keyword Keyword { get; set; } = null!;
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual ApplicationUser AssignedBy { get; set; } = null!;
    }
}
