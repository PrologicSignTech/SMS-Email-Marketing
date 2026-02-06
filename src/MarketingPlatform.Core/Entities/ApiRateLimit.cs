namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents API rate limit configuration per user/tenant
    /// </summary>
    public class ApiRateLimit : BaseEntity
    {
        /// <summary>
        /// User ID (null for tenant-wide limits)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Tenant ID (null for user-specific limits)
        /// </summary>
        public string? TenantId { get; set; }
        
        /// <summary>
        /// Endpoint pattern (e.g., "/api/messages/*", "/api/messages/bulk")
        /// </summary>
        public string EndpointPattern { get; set; } = string.Empty;
        
        /// <summary>
        /// Maximum requests per time window
        /// </summary>
        public int MaxRequests { get; set; }
        
        /// <summary>
        /// Time window in seconds
        /// </summary>
        public int TimeWindowSeconds { get; set; }
        
        /// <summary>
        /// Current request count in the current window
        /// </summary>
        public int CurrentRequestCount { get; set; }
        
        /// <summary>
        /// Window start time
        /// </summary>
        public DateTime WindowStartTime { get; set; }
        
        /// <summary>
        /// Is this limit active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Priority (higher = checked first)
        /// </summary>
        public int Priority { get; set; } = 0;
        
        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
    }
}
