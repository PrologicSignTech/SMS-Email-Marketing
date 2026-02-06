using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ContactConsent : BaseEntity
    {
        public int ContactId { get; set; }
        public ConsentChannel Channel { get; set; }
        public ConsentStatus Status { get; set; }
        public ConsentSource Source { get; set; }
        public DateTime ConsentDate { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedDate { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
    }
}
