using MarketingPlatform.Application.DTOs.RateLimit;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IRateLimitService
    {
        // ===== Frequency Control Methods (existing) =====
        
        /// <summary>
        /// Check if a message can be sent to a contact based on frequency control
        /// </summary>
        Task<bool> CanSendMessageAsync(int contactId, string userId);

        /// <summary>
        /// Record that a message was sent to a contact
        /// </summary>
        Task RecordMessageSentAsync(int contactId, string userId);

        /// <summary>
        /// Get frequency control settings for a contact
        /// </summary>
        Task<Core.Entities.FrequencyControl?> GetFrequencyControlAsync(int contactId, string userId);

        /// <summary>
        /// Update frequency control settings for a contact
        /// </summary>
        Task UpdateFrequencyControlAsync(int contactId, string userId, int maxPerDay, int maxPerWeek, int maxPerMonth);

        /// <summary>
        /// Reset daily counters for all contacts (scheduled job)
        /// </summary>
        Task ResetDailyCountersAsync();

        /// <summary>
        /// Reset weekly counters for all contacts (scheduled job)
        /// </summary>
        Task ResetWeeklyCountersAsync();

        /// <summary>
        /// Reset monthly counters for all contacts (scheduled job)
        /// </summary>
        Task ResetMonthlyCountersAsync();

        // ===== API Rate Limiting Methods (new) =====

        /// <summary>
        /// Check if an API request is within rate limits
        /// </summary>
        Task<RateLimitStatusDto> CheckApiRateLimitAsync(string userId, string? tenantId, string endpoint, string httpMethod);

        /// <summary>
        /// Record an API request
        /// </summary>
        Task RecordApiRequestAsync(string userId, string? tenantId, string endpoint, string httpMethod);

        /// <summary>
        /// Log a rate limit violation
        /// </summary>
        Task LogRateLimitViolationAsync(string userId, string? tenantId, string endpoint, string httpMethod, string ipAddress, string rateLimitRule, int requestCount, int maxRequests, int timeWindowSeconds, int retryAfterSeconds);

        /// <summary>
        /// Get API rate limit configuration for a specific user/tenant
        /// </summary>
        Task<List<ApiRateLimitDto>> GetApiRateLimitsAsync(string? userId, string? tenantId);

        /// <summary>
        /// Create a new API rate limit rule
        /// </summary>
        Task<ApiRateLimitDto> CreateApiRateLimitAsync(CreateApiRateLimitDto dto);

        /// <summary>
        /// Update an existing API rate limit rule
        /// </summary>
        Task<ApiRateLimitDto> UpdateApiRateLimitAsync(int id, UpdateApiRateLimitDto dto);

        /// <summary>
        /// Delete an API rate limit rule
        /// </summary>
        Task DeleteApiRateLimitAsync(int id);

        /// <summary>
        /// Get rate limit logs for monitoring
        /// </summary>
        Task<List<RateLimitLogDto>> GetRateLimitLogsAsync(string? userId, DateTime? startDate, DateTime? endDate, int pageSize = 100);

        // ===== Provider Rate Limiting Methods (new) =====

        /// <summary>
        /// Check if a provider request is within rate limits
        /// </summary>
        Task<bool> CheckProviderRateLimitAsync(string providerName, string providerType, string? userId = null);

        /// <summary>
        /// Record a provider request
        /// </summary>
        Task RecordProviderRequestAsync(string providerName, string providerType, string? userId = null);
    }
}
