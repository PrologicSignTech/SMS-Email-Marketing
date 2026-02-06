namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Landing page FAQ entity
    /// </summary>
    public class LandingFaq : BaseEntity
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty; // Bootstrap icon class (e.g., "bi-gift")
        public string IconColor { get; set; } = string.Empty; // Color class (e.g., "primary", "warning")
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool ShowOnLanding { get; set; } = true;
        public string? Category { get; set; } // Optional grouping
    }
}
