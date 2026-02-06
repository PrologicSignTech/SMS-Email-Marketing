using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ChannelRoutingConfig : BaseEntity
    {
        public ChannelType Channel { get; set; }
        public string PrimaryProvider { get; set; } = string.Empty;
        public string? FallbackProvider { get; set; }
        public RoutingStrategy RoutingStrategy { get; set; } = RoutingStrategy.Primary;
        public bool EnableFallback { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public RetryStrategy RetryStrategy { get; set; } = RetryStrategy.Exponential;
        public int InitialRetryDelaySeconds { get; set; } = 60;
        public int MaxRetryDelaySeconds { get; set; } = 3600;
        public decimal? CostThreshold { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1;
        public string? AdditionalSettings { get; set; } // JSON for extra config
    }
}
