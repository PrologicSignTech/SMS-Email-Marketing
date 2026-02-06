using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class MessageProvider : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ProviderType Type { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? Configuration { get; set; } // JSON
        public bool IsActive { get; set; } = true;
        public bool IsPrimary { get; set; } = false;
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;
        public DateTime? LastHealthCheck { get; set; }

        // Navigation properties
        public virtual ICollection<ProviderLog> Logs { get; set; } = new List<ProviderLog>();
    }
}
