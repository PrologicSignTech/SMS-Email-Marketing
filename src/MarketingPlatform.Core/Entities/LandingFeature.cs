namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Landing page feature card entity
    /// </summary>
    public class LandingFeature : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string DetailedDescription { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty; // Bootstrap icon class (e.g., "bi-broadcast")
        public string ColorClass { get; set; } = string.Empty; // Color class (e.g., "primary", "success")
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool ShowOnLanding { get; set; } = true;

        // Call to action
        public string? CallToActionText { get; set; }
        public string? CallToActionUrl { get; set; }

        // Statistics for detail page hero (not shown on flip cards)
        public string? StatTitle1 { get; set; }
        public string? StatValue1 { get; set; }
        public string? StatTitle2 { get; set; }
        public string? StatValue2 { get; set; }
        public string? StatTitle3 { get; set; }
        public string? StatValue3 { get; set; }

        // Media for detail page
        public string? HeaderImageUrl { get; set; } // Hero image for detail page
        public string? VideoUrl { get; set; } // Video URL (YouTube, Vimeo, or direct)
        public string? GalleryImages { get; set; } // JSON array of image URLs

        // Contact information for detail page
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactMessage { get; set; } // Pre-filled message or description
    }
}
