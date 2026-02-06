namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Tracks provider-specific rate limits (SMS, Email, etc.)
    /// </summary>
    public class ProviderRateLimit : BaseEntity
    {
        /// <summary>
        /// Provider name (e.g., "Twilio", "SendGrid")
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;
        
        /// <summary>
        /// Provider type (SMS, Email, MMS)
        /// </summary>
        public string ProviderType { get; set; } = string.Empty;
        
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
        /// User/Tenant ID (if provider limits are per-user)
        /// </summary>
        public string? UserId { get; set; }
    }
}
