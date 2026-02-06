using System;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Security and compliance badge for landing page
    /// </summary>
    public class SecurityBadge
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;           // "GDPR"
        public string Subtitle { get; set; } = string.Empty;        // "Compliant"
        public string IconUrl { get; set; } = string.Empty;         // "/images/badges/gdpr.svg"
        public string Description { get; set; } = string.Empty;     // Full description
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ShowOnLanding { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
