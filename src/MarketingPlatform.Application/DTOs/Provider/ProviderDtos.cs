using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Provider
{
    public class ProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProviderType Type { get; set; }
        public string TypeName => Type.ToString();
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public HealthStatus HealthStatus { get; set; }
        public string HealthStatusName => HealthStatus.ToString();
        public DateTime? LastHealthCheck { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProviderDetailDto : ProviderDto
    {
        public string? ApiKey { get; set; }
        public string? Configuration { get; set; }
    }

    public class CreateProviderDto
    {
        public string Name { get; set; } = string.Empty;
        public ProviderType Type { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? Configuration { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPrimary { get; set; } = false;
    }

    public class UpdateProviderDto
    {
        public string Name { get; set; } = string.Empty;
        public ProviderType Type { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? Configuration { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
    }
}
