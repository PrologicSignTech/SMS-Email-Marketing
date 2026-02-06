using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ComplianceAuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public int? CampaignId { get; set; }
        public ComplianceActionType ActionType { get; set; }
        public ConsentChannel Channel { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Metadata { get; set; } // JSON for additional context
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Contact? Contact { get; set; }
        public virtual Campaign? Campaign { get; set; }
    }
}
