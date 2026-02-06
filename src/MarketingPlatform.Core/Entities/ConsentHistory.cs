using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ConsentHistory : BaseEntity
    {
        public int ContactId { get; set; }
        public bool ConsentGiven { get; set; }
        public string? ConsentType { get; set; }
        public ConsentChannel? Channel { get; set; }
        public ConsentSource? Source { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ConsentDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
    }
}
