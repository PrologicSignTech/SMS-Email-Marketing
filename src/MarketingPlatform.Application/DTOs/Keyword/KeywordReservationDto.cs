using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordReservationDto
    {
        public int Id { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        public string RequestedByUserId { get; set; } = string.Empty;
        public string? RequestedByUserName { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public ReservationStatus Status { get; set; }
        public string? Purpose { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateKeywordReservationDto
    {
        public string KeywordText { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Priority { get; set; } = 0;
    }

    public class UpdateKeywordReservationDto
    {
        public ReservationStatus Status { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? RejectionReason { get; set; }
        public int Priority { get; set; }
    }
}
