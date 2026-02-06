namespace MarketingPlatform.Core.Entities
{
    public class URLClick : BaseEntity
    {
        public int URLShortenerId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual URLShortener URLShortener { get; set; } = null!;
    }
}
