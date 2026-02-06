namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Logs rate limit violations for monitoring and audit
    /// </summary>
    public class RateLimitLog : BaseEntity
    {
        /// <summary>
        /// User ID who triggered the rate limit
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Tenant ID (if applicable)
        /// </summary>
        public string? TenantId { get; set; }
        
        /// <summary>
        /// Endpoint that was called
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;
        
        /// <summary>
        /// HTTP method
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// IP address of the client
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Rate limit rule that was triggered
        /// </summary>
        public string RateLimitRule { get; set; } = string.Empty;
        
        /// <summary>
        /// Current request count when limit was hit
        /// </summary>
        public int RequestCount { get; set; }
        
        /// <summary>
        /// Maximum allowed requests
        /// </summary>
        public int MaxRequests { get; set; }
        
        /// <summary>
        /// Time window in seconds
        /// </summary>
        public int TimeWindowSeconds { get; set; }
        
        /// <summary>
        /// Timestamp when rate limit was triggered
        /// </summary>
        public DateTime TriggeredAt { get; set; }
        
        /// <summary>
        /// Retry-After time in seconds
        /// </summary>
        public int RetryAfterSeconds { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
