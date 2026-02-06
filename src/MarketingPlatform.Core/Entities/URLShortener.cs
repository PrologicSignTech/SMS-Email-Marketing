namespace MarketingPlatform.Core.Entities
{
    public class URLShortener : BaseEntity
    {
        public int CampaignId { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; } = 0;

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual ICollection<URLClick> Clicks { get; set; } = new List<URLClick>();
    }
}
