namespace MarketingPlatform.Core.Entities
{
    public class UsageTracking : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int SMSUsed { get; set; } = 0;
        public int MMSUsed { get; set; } = 0;
        public int EmailUsed { get; set; } = 0;
        public int ContactsUsed { get; set; } = 0;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
