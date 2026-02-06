namespace MarketingPlatform.Core.Enums
{
    public enum RoutingStrategy
    {
        Primary,
        Fallback,
        RoundRobin,
        LeastCost,
        HighestReliability
    }

    public enum ProviderStatus
    {
        Active,
        Inactive,
        Degraded,
        Failed
    }

    public enum RetryStrategy
    {
        None,
        Linear,
        Exponential,
        Custom
    }

    public enum FallbackReason
    {
        PrimaryFailed,
        ProviderUnavailable,
        RateLimitExceeded,
        CostThreshold,
        QualityThreshold
    }
}
