using System;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Use case/industry example for landing page
    /// </summary>
    public class UseCase
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi-lightbulb";  // Bootstrap icon class
        public string? Industry { get; set; }                     // e.g., "E-commerce", "Healthcare"
        public string? ImageUrl { get; set; }                     // Featured image
        public string? ResultsText { get; set; }                  // e.g., "300% increase in engagement"
        public string ColorClass { get; set; } = "primary";       // primary, success, info, warning, etc.
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
