namespace MarketingPlatform.Application.DTOs.URL
{
    /// <summary>
    /// DTO for creating a shortened URL
    /// </summary>
    public class CreateShortenedUrlDto
    {
        /// <summary>
        /// Campaign ID this URL belongs to
        /// </summary>
        public int CampaignId { get; set; }

        /// <summary>
        /// Original long URL to shorten
        /// </summary>
        public string OriginalUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom short code (if not provided, will be auto-generated)
        /// </summary>
        public string? CustomShortCode { get; set; }
    }
}
