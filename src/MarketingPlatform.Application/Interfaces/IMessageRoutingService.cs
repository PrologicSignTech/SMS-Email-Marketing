using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IMessageRoutingService
    {
        /// <summary>
        /// Routes a message through the appropriate channel and provider
        /// </summary>
        Task<(bool Success, string? ExternalId, string? Error, decimal? Cost, int AttemptNumber)> RouteMessageAsync(
            CampaignMessage message);

        /// <summary>
        /// Determines if a message should be retried based on retry strategy
        /// </summary>
        Task<(bool ShouldRetry, int DelaySeconds)> ShouldRetryMessageAsync(CampaignMessage message);

        /// <summary>
        /// Gets the next retry delay based on retry strategy
        /// </summary>
        Task<int> CalculateRetryDelayAsync(int attemptNumber, ChannelType channel);

        /// <summary>
        /// Attempts to send through fallback channel if primary fails
        /// </summary>
        Task<(bool Success, string? ExternalId, string? Error, decimal? Cost, FallbackReason? Reason)> TryFallbackChannelAsync(
            CampaignMessage message, 
            string primaryError);

        /// <summary>
        /// Logs delivery attempt with detailed information
        /// </summary>
        Task LogDeliveryAttemptAsync(
            int campaignMessageId,
            int attemptNumber,
            ChannelType channel,
            string? providerName,
            bool success,
            string? externalId,
            string? error,
            string? errorCode,
            decimal? cost,
            int responseTimeMs,
            FallbackReason? fallbackReason = null);

        // Routing Configuration Management
        /// <summary>
        /// Get all routing configurations
        /// </summary>
        Task<List<ChannelRoutingConfig>> GetAllConfigsAsync();

        /// <summary>
        /// Get routing configuration by ID
        /// </summary>
        Task<ChannelRoutingConfig?> GetConfigByIdAsync(int id);

        /// <summary>
        /// Get active routing configuration for a specific channel
        /// </summary>
        Task<ChannelRoutingConfig?> GetConfigByChannelAsync(ChannelType channel);

        /// <summary>
        /// Create a new routing configuration
        /// </summary>
        Task<ChannelRoutingConfig> CreateConfigAsync(ChannelRoutingConfig config);

        /// <summary>
        /// Update an existing routing configuration
        /// </summary>
        Task<ChannelRoutingConfig?> UpdateConfigAsync(int id, ChannelRoutingConfig updatedConfig);

        /// <summary>
        /// Delete a routing configuration
        /// </summary>
        Task<bool> DeleteConfigAsync(int id);

        // Delivery Attempt Management
        /// <summary>
        /// Get delivery attempts for a specific message
        /// </summary>
        Task<List<MessageDeliveryAttempt>> GetDeliveryAttemptsAsync(int messageId);

        /// <summary>
        /// Get delivery statistics for a specific channel
        /// </summary>
        Task<object> GetChannelStatsAsync(ChannelType channel, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get overall delivery statistics
        /// </summary>
        Task<object> GetOverallStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
