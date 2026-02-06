namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents footer configuration for the landing page
    /// </summary>
    public class FooterSettings : BaseEntity
    {
        /// <summary>
        /// Company name displayed in footer
        /// </summary>
        public string CompanyName { get; set; } = "Marketing Platform";

        /// <summary>
        /// Company tagline/description
        /// </summary>
        public string CompanyDescription { get; set; } = string.Empty;

        /// <summary>
        /// Office address line 1
        /// </summary>
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Office address line 2 (city, state, zip)
        /// </summary>
        public string AddressLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Contact email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Business hours
        /// </summary>
        public string BusinessHours { get; set; } = string.Empty;

        /// <summary>
        /// Google Maps embed URL
        /// </summary>
        public string? MapEmbedUrl { get; set; }

        /// <summary>
        /// Facebook URL
        /// </summary>
        public string? FacebookUrl { get; set; }

        /// <summary>
        /// Twitter/X URL
        /// </summary>
        public string? TwitterUrl { get; set; }

        /// <summary>
        /// LinkedIn URL
        /// </summary>
        public string? LinkedInUrl { get; set; }

        /// <summary>
        /// Instagram URL
        /// </summary>
        public string? InstagramUrl { get; set; }

        /// <summary>
        /// YouTube URL
        /// </summary>
        public string? YouTubeUrl { get; set; }

        /// <summary>
        /// Copyright text
        /// </summary>
        public string CopyrightText { get; set; } = string.Empty;

        /// <summary>
        /// Newsletter enabled
        /// </summary>
        public bool ShowNewsletter { get; set; } = true;

        /// <summary>
        /// Newsletter title
        /// </summary>
        public string NewsletterTitle { get; set; } = "Subscribe to Our Newsletter";

        /// <summary>
        /// Newsletter description
        /// </summary>
        public string NewsletterDescription { get; set; } = "Get the latest updates, tips and exclusive offers delivered to your inbox.";

        /// <summary>
        /// Whether to show the map
        /// </summary>
        public bool ShowMap { get; set; } = true;

        /// <summary>
        /// Whether these settings are active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
