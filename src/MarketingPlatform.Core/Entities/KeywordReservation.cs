using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class KeywordReservation : BaseEntity
    {
        public string KeywordText { get; set; } = string.Empty;
        public string RequestedByUserId { get; set; } = string.Empty;
        public string? ApprovedByUserId { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string? Purpose { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public int Priority { get; set; } = 0; // Higher priority wins in conflicts

        // Navigation properties
        public virtual ApplicationUser RequestedBy { get; set; } = null!;
        public virtual ApplicationUser? ApprovedBy { get; set; }
    }
}
