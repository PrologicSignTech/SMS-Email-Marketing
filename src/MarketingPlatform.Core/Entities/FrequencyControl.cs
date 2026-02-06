namespace MarketingPlatform.Core.Entities
{
    public class FrequencyControl : BaseEntity
    {
        public int ContactId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MaxMessagesPerDay { get; set; } = 5;
        public int MaxMessagesPerWeek { get; set; } = 20;
        public int MaxMessagesPerMonth { get; set; } = 50;
        public DateTime? LastMessageSentAt { get; set; }
        public int MessagesSentToday { get; set; } = 0;
        public int MessagesSentThisWeek { get; set; } = 0;
        public int MessagesSentThisMonth { get; set; } = 0;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
