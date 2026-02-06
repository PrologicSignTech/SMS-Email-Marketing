using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Compliance
{
    public class ConsentRequestDto
    {
        public int ContactId { get; set; }
        public ConsentChannel Channel { get; set; }
        public ConsentSource Source { get; set; }
        public bool ConsentGiven { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkConsentRequestDto
    {
        public List<int> ContactIds { get; set; } = new();
        public ConsentChannel Channel { get; set; }
        public ConsentSource Source { get; set; }
        public bool ConsentGiven { get; set; }
        public string? IpAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class ConsentStatusDto
    {
        public int ContactId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool SmsOptIn { get; set; }
        public bool MmsOptIn { get; set; }
        public bool EmailOptIn { get; set; }
        public DateTime? SmsOptInDate { get; set; }
        public DateTime? MmsOptInDate { get; set; }
        public DateTime? EmailOptInDate { get; set; }
    }

    public class ContactConsentDto
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public ConsentChannel Channel { get; set; }
        public ConsentStatus Status { get; set; }
        public ConsentSource Source { get; set; }
        public DateTime ConsentDate { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string? IpAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class ConsentHistoryDto
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public bool ConsentGiven { get; set; }
        public string? ConsentType { get; set; }
        public ConsentChannel? Channel { get; set; }
        public ConsentSource? Source { get; set; }
        public string? IpAddress { get; set; }
        public DateTime ConsentDate { get; set; }
    }
}
