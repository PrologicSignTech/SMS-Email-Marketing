namespace MarketingPlatform.Application.DTOs.RateLimit
{
    public class ApiRateLimitDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? TenantId { get; set; }
        public string EndpointPattern { get; set; } = string.Empty;
        public int MaxRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateApiRateLimitDto
    {
        public string? UserId { get; set; }
        public string? TenantId { get; set; }
        public string EndpointPattern { get; set; } = string.Empty;
        public int MaxRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
    }

    public class UpdateApiRateLimitDto
    {
        public int MaxRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
    }

    public class RateLimitLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? TenantId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string RateLimitRule { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public int MaxRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public DateTime TriggeredAt { get; set; }
        public int RetryAfterSeconds { get; set; }
    }

    public class RateLimitStatusDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int MaxRequests { get; set; }
        public int RemainingRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public DateTime WindowResetTime { get; set; }
        public bool IsLimited { get; set; }
        public int? RetryAfterSeconds { get; set; }
    }

    public class ProviderRateLimitDto
    {
        public int Id { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public int MaxRequests { get; set; }
        public int TimeWindowSeconds { get; set; }
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
