using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class TaxConfiguration : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public TaxType Type { get; set; }
        public string? RegionCode { get; set; } // Apply to specific region (null for global)
        public decimal Rate { get; set; } // Percentage rate (e.g., 8.5 for 8.5%)
        public decimal? FlatAmount { get; set; } // Fixed amount if not percentage-based
        public bool IsPercentage { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0; // Order of application
        public string? Configuration { get; set; } // JSON for additional settings
    }
}
