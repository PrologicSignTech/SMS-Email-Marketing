using System;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Customer testimonial for landing page
    /// </summary>
    public class Testimonial
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerTitle { get; set; }  // e.g., "CEO", "Marketing Director"
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }    // URL to company logo
        public string? AvatarUrl { get; set; }       // Customer photo
        public int Rating { get; set; } = 5;         // 1-5 star rating
        public string TestimonialText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
