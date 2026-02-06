namespace MarketingPlatform.Application.DTOs.URL
{
    /// <summary>
    /// DTO for shortened URL information
    /// </summary>
    public class UrlShortenerDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
