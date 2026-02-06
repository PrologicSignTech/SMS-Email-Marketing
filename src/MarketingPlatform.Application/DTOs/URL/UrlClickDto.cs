namespace MarketingPlatform.Application.DTOs.URL
{
    /// <summary>
    /// DTO for tracking a URL click
    /// </summary>
    public class UrlClickDto
    {
        /// <summary>
        /// IP address of the user who clicked
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string from browser
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Referrer URL
        /// </summary>
        public string? Referrer { get; set; }
    }
}
