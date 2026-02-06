using System;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Trusted company logo for "Trusted by" section
    /// </summary>
    public class TrustedCompany
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;       // URL to company logo image
        public string? WebsiteUrl { get; set; }                    // Optional link to company website
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
