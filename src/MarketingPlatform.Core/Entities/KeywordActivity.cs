namespace MarketingPlatform.Core.Entities
{
    public class KeywordActivity : BaseEntity
    {
        public int KeywordId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string IncomingMessage { get; set; } = string.Empty;
        public string? ResponseSent { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Keyword Keyword { get; set; } = null!;
    }
}
