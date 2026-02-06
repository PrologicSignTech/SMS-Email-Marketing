using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordConflictDto
    {
        public int Id { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        public string RequestingUserId { get; set; } = string.Empty;
        public string? RequestingUserName { get; set; }
        public string ExistingUserId { get; set; } = string.Empty;
        public string? ExistingUserName { get; set; }
        public string? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ResolveKeywordConflictDto
    {
        public ReservationStatus Status { get; set; }
        public string? Resolution { get; set; }
    }
}
