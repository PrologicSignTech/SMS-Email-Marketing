namespace MarketingPlatform.Application.DTOs.URL
{
    /// <summary>
    /// DTO for click statistics on a shortened URL
    /// </summary>
    public class UrlClickStatsDto
    {
        public int UrlShortenerId { get; set; }
        public string ShortUrl { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int TotalClicks { get; set; }
        public int UniqueClicks { get; set; }
        public DateTime? FirstClickAt { get; set; }
        public DateTime? LastClickAt { get; set; }
        public List<ClickByDateDto> ClicksByDate { get; set; } = new();
        public List<ClickByReferrerDto> TopReferrers { get; set; } = new();
    }

    public class ClickByDateDto
    {
        public DateTime Date { get; set; }
        public int ClickCount { get; set; }
    }

    public class ClickByReferrerDto
    {
        public string Referrer { get; set; } = string.Empty;
        public int ClickCount { get; set; }
    }
}
