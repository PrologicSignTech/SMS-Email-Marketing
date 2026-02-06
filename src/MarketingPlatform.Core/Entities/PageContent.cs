namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Stores content for static pages like Privacy Policy and Terms of Service
    /// </summary>
    public class PageContent : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the page (e.g., "privacy-policy", "terms-of-service")
        /// </summary>
        public string PageKey { get; set; } = string.Empty;

        /// <summary>
        /// Display title of the page
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Main content of the page (HTML supported)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional meta description for SEO
        /// </summary>
        public string? MetaDescription { get; set; }

        /// <summary>
        /// List of image URLs/paths associated with this page (stored as JSON)
        /// </summary>
        public string? ImageUrls { get; set; }

        /// <summary>
        /// Whether this page is currently published and visible to users
        /// </summary>
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// User ID of the person who last modified this page
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Navigation property to the user who last modified this page
        /// </summary>
        public virtual ApplicationUser? LastModifiedByUser { get; set; }
    }
}
