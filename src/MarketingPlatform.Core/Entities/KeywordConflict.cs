using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class KeywordConflict : BaseEntity
    {
        public string KeywordText { get; set; } = string.Empty;
        public string RequestingUserId { get; set; } = string.Empty;
        public string ExistingUserId { get; set; } = string.Empty;
        public string? ResolvedByUserId { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ApplicationUser RequestingUser { get; set; } = null!;
        public virtual ApplicationUser ExistingUser { get; set; } = null!;
        public virtual ApplicationUser? ResolvedBy { get; set; }
    }
}
