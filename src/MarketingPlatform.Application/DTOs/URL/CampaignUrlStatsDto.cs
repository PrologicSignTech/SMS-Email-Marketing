namespace MarketingPlatform.Application.DTOs.URL
{
    /// <summary>
    /// DTO for campaign URL statistics
    /// </summary>
    public class CampaignUrlStatsDto
    {
        public int CampaignId { get; set; }
        public int TotalUrls { get; set; }
        public int TotalClicks { get; set; }
        public int UniqueClicks { get; set; }
        public List<UrlShortenerStatsDto> TopUrls { get; set; } = new();
    }

    public class UrlShortenerStatsDto
    {
        public int Id { get; set; }
        public string ShortUrl { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }
    }
}
