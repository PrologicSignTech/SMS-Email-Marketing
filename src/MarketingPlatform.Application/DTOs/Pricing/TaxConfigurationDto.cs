using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Pricing
{
    public class TaxConfigurationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TaxType Type { get; set; }
        public string? RegionCode { get; set; }
        public decimal Rate { get; set; }
        public decimal? FlatAmount { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public string? Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaxConfigurationDto
    {
        public string Name { get; set; } = string.Empty;
        public TaxType Type { get; set; }
        public string? RegionCode { get; set; }
        public decimal Rate { get; set; }
        public decimal? FlatAmount { get; set; }
        public bool IsPercentage { get; set; } = true;
        public int Priority { get; set; } = 0;
        public string? Configuration { get; set; }
    }

    public class UpdateTaxConfigurationDto
    {
        public string Name { get; set; } = string.Empty;
        public TaxType Type { get; set; }
        public string? RegionCode { get; set; }
        public decimal Rate { get; set; }
        public decimal? FlatAmount { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public string? Configuration { get; set; }
    }
}
