namespace MarketingPlatform.Core.Entities
{
    public class ContactEngagement : BaseEntity
    {
        public int ContactId { get; set; }
        public int TotalMessagesSent { get; set; } = 0;
        public int TotalMessagesDelivered { get; set; } = 0;
        public int TotalClicks { get; set; } = 0;
        public DateTime? LastEngagementDate { get; set; }
        public decimal EngagementScore { get; set; } = 0;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
    }
}
