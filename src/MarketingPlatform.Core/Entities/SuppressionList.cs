using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class SuppressionList : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string PhoneOrEmail { get; set; } = string.Empty;
        public SuppressionType Type { get; set; }
        public string? Reason { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
