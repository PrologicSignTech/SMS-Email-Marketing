using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MarketingPlatform.Application.Services
{
    public class MessageRoutingService : IMessageRoutingService
    {
        private readonly IRepository<ChannelRoutingConfig> _routingConfigRepository;
        private readonly IRepository<MessageDeliveryAttempt> _deliveryAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISMSProvider _smsProvider;
        private readonly IMMSProvider _mmsProvider;
        private readonly IEmailProvider _emailProvider;
        private readonly ILogger<MessageRoutingService> _logger;

        public MessageRoutingService(
            IRepository<ChannelRoutingConfig> routingConfigRepository,
            IRepository<MessageDeliveryAttempt> deliveryAttemptRepository,
            IUnitOfWork unitOfWork,
            ISMSProvider smsProvider,
            IMMSProvider mmsProvider,
            IEmailProvider emailProvider,
            ILogger<MessageRoutingService> logger)
        {
            _routingConfigRepository = routingConfigRepository;
            _deliveryAttemptRepository = deliveryAttemptRepository;
            _unitOfWork = unitOfWork;
            _smsProvider = smsProvider;
            _mmsProvider = mmsProvider;
            _emailProvider = emailProvider;
            _logger = logger;
        }

        public async Task<(bool Success, string? ExternalId, string? Error, decimal? Cost, int AttemptNumber)> RouteMessageAsync(
            CampaignMessage message)
        {
            var stopwatch = Stopwatch.StartNew();
            var attemptNumber = message.RetryCount + 1;
            
            _logger.LogInformation(
                "Routing message {MessageId} (Attempt {AttemptNumber}) via {Channel}",
                message.Id, attemptNumber, message.Channel);

            try
            {
                // Get routing configuration for this channel
                var config = await GetRoutingConfigAsync(message.Channel);
                
                bool success;
                string? externalId;
                string? error;
                decimal? cost;
                string providerName;

                // Route based on channel
                switch (message.Channel)
                {
                    case ChannelType.SMS:
                        providerName = config?.PrimaryProvider ?? "MockSMSProvider";
                        (success, externalId, error, cost) = await _smsProvider.SendSMSAsync(
                            message.Recipient, message.MessageBody ?? string.Empty);
                        break;

                    case ChannelType.MMS:
                        providerName = config?.PrimaryProvider ?? "MockMMSProvider";
                        var mediaUrls = !string.IsNullOrEmpty(message.MediaUrls)
                            ? JsonConvert.DeserializeObject<List<string>>(message.MediaUrls) ?? new List<string>()
                            : new List<string>();
                        (success, externalId, error, cost) = await _mmsProvider.SendMMSAsync(
                            message.Recipient, message.MessageBody ?? string.Empty, mediaUrls);
                        break;

                    case ChannelType.Email:
                        providerName = config?.PrimaryProvider ?? "MockEmailProvider";
                        (success, externalId, error, cost) = await _emailProvider.SendEmailAsync(
                            message.Recipient, 
                            message.Subject ?? string.Empty, 
                            message.MessageBody ?? string.Empty, 
                            message.HTMLContent);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported channel type: {message.Channel}");
                }

                stopwatch.Stop();

                // Log the delivery attempt
                await LogDeliveryAttemptAsync(
                    message.Id,
                    attemptNumber,
                    message.Channel,
                    providerName,
                    success,
                    externalId,
                    error,
                    null, // errorCode - can be enhanced
                    cost,
                    (int)stopwatch.ElapsedMilliseconds);

                if (success)
                {
                    _logger.LogInformation(
                        "Message {MessageId} successfully routed via {Channel} (Provider: {Provider}, ExternalId: {ExternalId}, Time: {TimeMs}ms)",
                        message.Id, message.Channel, providerName, externalId, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning(
                        "Message {MessageId} routing failed via {Channel} (Provider: {Provider}, Error: {Error}, Time: {TimeMs}ms)",
                        message.Id, message.Channel, providerName, error, stopwatch.ElapsedMilliseconds);
                    
                    // Try fallback if enabled and available
                    if (config?.EnableFallback == true && !string.IsNullOrEmpty(config.FallbackProvider))
                    {
                        var fallbackResult = await TryFallbackChannelAsync(message, error ?? "Unknown error");
                        if (fallbackResult.Success)
                        {
                            success = fallbackResult.Success;
                            externalId = fallbackResult.ExternalId;
                            error = null;
                            cost = fallbackResult.Cost;
                        }
                    }
                }

                return (success, externalId, error, cost, attemptNumber);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Exception while routing message {MessageId}", message.Id);
                
                await LogDeliveryAttemptAsync(
                    message.Id,
                    attemptNumber,
                    message.Channel,
                    "Unknown",
                    false,
                    null,
                    ex.Message,
                    ex.GetType().Name,
                    null,
                    (int)stopwatch.ElapsedMilliseconds);

                return (false, null, ex.Message, null, attemptNumber);
            }
        }

        public async Task<(bool ShouldRetry, int DelaySeconds)> ShouldRetryMessageAsync(CampaignMessage message)
        {
            var config = await GetRoutingConfigAsync(message.Channel);
            var maxRetries = config?.MaxRetries ?? message.MaxRetries;

            if (message.RetryCount >= maxRetries)
            {
                _logger.LogInformation(
                    "Message {MessageId} has reached max retries ({RetryCount}/{MaxRetries})",
                    message.Id, message.RetryCount, maxRetries);
                return (false, 0);
            }

            var delaySeconds = await CalculateRetryDelayAsync(message.RetryCount + 1, message.Channel);
            
            _logger.LogInformation(
                "Message {MessageId} will be retried (Attempt {NextAttempt}/{MaxRetries}) after {DelaySeconds}s",
                message.Id, message.RetryCount + 1, maxRetries, delaySeconds);

            return (true, delaySeconds);
        }

        public async Task<int> CalculateRetryDelayAsync(int attemptNumber, ChannelType channel)
        {
            var config = await GetRoutingConfigAsync(channel);
            var retryStrategy = config?.RetryStrategy ?? RetryStrategy.Exponential;
            var initialDelay = config?.InitialRetryDelaySeconds ?? 60;
            var maxDelay = config?.MaxRetryDelaySeconds ?? 3600;

            int delaySeconds;

            switch (retryStrategy)
            {
                case RetryStrategy.None:
                    delaySeconds = 0;
                    break;

                case RetryStrategy.Linear:
                    delaySeconds = initialDelay * attemptNumber;
                    break;

                case RetryStrategy.Exponential:
                    // Exponential backoff: initial * (2 ^ (attempt - 1))
                    delaySeconds = initialDelay * (int)Math.Pow(2, attemptNumber - 1);
                    break;

                case RetryStrategy.Custom:
                    // Can be enhanced to use custom logic from config
                    delaySeconds = initialDelay * attemptNumber;
                    break;

                default:
                    delaySeconds = initialDelay;
                    break;
            }

            // Cap at max delay
            delaySeconds = Math.Min(delaySeconds, maxDelay);

            return delaySeconds;
        }

        public async Task<(bool Success, string? ExternalId, string? Error, decimal? Cost, FallbackReason? Reason)> TryFallbackChannelAsync(
            CampaignMessage message, 
            string primaryError)
        {
            _logger.LogInformation(
                "Attempting fallback for message {MessageId}. Primary error: {Error}",
                message.Id, primaryError);

            var config = await GetRoutingConfigAsync(message.Channel);
            
            if (config == null || !config.EnableFallback || string.IsNullOrEmpty(config.FallbackProvider))
            {
                _logger.LogWarning(
                    "No fallback configured for channel {Channel}",
                    message.Channel);
                return (false, null, "No fallback configured", null, null);
            }

            var fallbackReason = DetermineFallbackReason(primaryError);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // For simplicity, using same channel with fallback provider
                // In production, this could route to alternative channels (SMS -> Email, etc.)
                bool success;
                string? externalId;
                string? error;
                decimal? cost;

                switch (message.Channel)
                {
                    case ChannelType.SMS:
                        (success, externalId, error, cost) = await _smsProvider.SendSMSAsync(
                            message.Recipient, message.MessageBody ?? string.Empty);
                        break;

                    case ChannelType.MMS:
                        var mediaUrls = !string.IsNullOrEmpty(message.MediaUrls)
                            ? JsonConvert.DeserializeObject<List<string>>(message.MediaUrls) ?? new List<string>()
                            : new List<string>();
                        (success, externalId, error, cost) = await _mmsProvider.SendMMSAsync(
                            message.Recipient, message.MessageBody ?? string.Empty, mediaUrls);
                        break;

                    case ChannelType.Email:
                        (success, externalId, error, cost) = await _emailProvider.SendEmailAsync(
                            message.Recipient,
                            message.Subject ?? string.Empty,
                            message.MessageBody ?? string.Empty,
                            message.HTMLContent);
                        break;

                    default:
                        return (false, null, $"Unsupported channel: {message.Channel}", null, fallbackReason);
                }

                stopwatch.Stop();

                // Log the fallback attempt
                await LogDeliveryAttemptAsync(
                    message.Id,
                    message.RetryCount + 1,
                    message.Channel,
                    config.FallbackProvider,
                    success,
                    externalId,
                    error,
                    null,
                    cost,
                    (int)stopwatch.ElapsedMilliseconds,
                    fallbackReason);

                if (success)
                {
                    _logger.LogInformation(
                        "Message {MessageId} successfully sent via fallback provider {Provider}",
                        message.Id, config.FallbackProvider);
                }
                else
                {
                    _logger.LogWarning(
                        "Message {MessageId} fallback also failed. Provider: {Provider}, Error: {Error}",
                        message.Id, config.FallbackProvider, error);
                }

                return (success, externalId, error, cost, fallbackReason);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, 
                    "Exception during fallback for message {MessageId}",
                    message.Id);

                await LogDeliveryAttemptAsync(
                    message.Id,
                    message.RetryCount + 1,
                    message.Channel,
                    config.FallbackProvider,
                    false,
                    null,
                    ex.Message,
                    ex.GetType().Name,
                    null,
                    (int)stopwatch.ElapsedMilliseconds,
                    fallbackReason);

                return (false, null, ex.Message, null, fallbackReason);
            }
        }

        public async Task LogDeliveryAttemptAsync(
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
            FallbackReason? fallbackReason = null)
        {
            try
            {
                var attempt = new MessageDeliveryAttempt
                {
                    CampaignMessageId = campaignMessageId,
                    AttemptNumber = attemptNumber,
                    Channel = channel,
                    ProviderName = providerName,
                    AttemptedAt = DateTime.UtcNow,
                    Success = success,
                    ExternalMessageId = externalId,
                    ErrorMessage = error,
                    ErrorCode = errorCode,
                    CostAmount = cost,
                    ResponseTimeMs = responseTimeMs,
                    FallbackReason = fallbackReason
                };

                await _deliveryAttemptRepository.AddAsync(attempt);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogDebug(
                    "Logged delivery attempt for message {MessageId}: Attempt {AttemptNumber}, Success: {Success}, Time: {TimeMs}ms",
                    campaignMessageId, attemptNumber, success, responseTimeMs);
            }
            catch (Exception ex)
            {
                // Don't let logging failures break the flow
                _logger.LogError(ex, 
                    "Failed to log delivery attempt for message {MessageId}",
                    campaignMessageId);
            }
        }

        private async Task<ChannelRoutingConfig?> GetRoutingConfigAsync(ChannelType channel)
        {
            try
            {
                var config = await _routingConfigRepository.GetQueryable()
                    .Where(c => c.Channel == channel && c.IsActive)
                    .OrderByDescending(c => c.Priority)
                    .FirstOrDefaultAsync();

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routing config for channel {Channel}", channel);
                return null;
            }
        }

        private FallbackReason DetermineFallbackReason(string error)
        {
            if (string.IsNullOrEmpty(error))
                return FallbackReason.PrimaryFailed;

            var errorLower = error.ToLowerInvariant();

            if (errorLower.Contains("rate limit") || errorLower.Contains("throttle"))
                return FallbackReason.RateLimitExceeded;

            if (errorLower.Contains("unavailable") || errorLower.Contains("timeout"))
                return FallbackReason.ProviderUnavailable;

            if (errorLower.Contains("cost") || errorLower.Contains("quota"))
                return FallbackReason.CostThreshold;

            return FallbackReason.PrimaryFailed;
        }

        // Routing Configuration Management Methods
        public async Task<List<ChannelRoutingConfig>> GetAllConfigsAsync()
        {
            return await _routingConfigRepository.GetQueryable()
                .OrderBy(c => c.Channel)
                .ThenByDescending(c => c.Priority)
                .ToListAsync();
        }

        public async Task<ChannelRoutingConfig?> GetConfigByIdAsync(int id)
        {
            return await _routingConfigRepository.GetByIdAsync(id);
        }

        public async Task<ChannelRoutingConfig?> GetConfigByChannelAsync(ChannelType channel)
        {
            return await _routingConfigRepository.GetQueryable()
                .Where(c => c.Channel == channel && c.IsActive)
                .OrderByDescending(c => c.Priority)
                .FirstOrDefaultAsync();
        }

        public async Task<ChannelRoutingConfig> CreateConfigAsync(ChannelRoutingConfig config)
        {
            config.CreatedAt = DateTime.UtcNow;
            await _routingConfigRepository.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();
            return config;
        }

        public async Task<ChannelRoutingConfig?> UpdateConfigAsync(int id, ChannelRoutingConfig updatedConfig)
        {
            var config = await _routingConfigRepository.GetByIdAsync(id);
            
            if (config == null)
                return null;

            // Update properties
            config.PrimaryProvider = updatedConfig.PrimaryProvider;
            config.FallbackProvider = updatedConfig.FallbackProvider;
            config.RoutingStrategy = updatedConfig.RoutingStrategy;
            config.EnableFallback = updatedConfig.EnableFallback;
            config.MaxRetries = updatedConfig.MaxRetries;
            config.RetryStrategy = updatedConfig.RetryStrategy;
            config.InitialRetryDelaySeconds = updatedConfig.InitialRetryDelaySeconds;
            config.MaxRetryDelaySeconds = updatedConfig.MaxRetryDelaySeconds;
            config.CostThreshold = updatedConfig.CostThreshold;
            config.IsActive = updatedConfig.IsActive;
            config.Priority = updatedConfig.Priority;
            config.AdditionalSettings = updatedConfig.AdditionalSettings;
            config.UpdatedAt = DateTime.UtcNow;

            _routingConfigRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            return config;
        }

        public async Task<bool> DeleteConfigAsync(int id)
        {
            var config = await _routingConfigRepository.GetByIdAsync(id);
            
            if (config == null)
                return false;

            _routingConfigRepository.Remove(config);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Delivery Attempt Management Methods
        public async Task<List<MessageDeliveryAttempt>> GetDeliveryAttemptsAsync(int messageId)
        {
            return await _deliveryAttemptRepository.GetQueryable()
                .Where(a => a.CampaignMessageId == messageId)
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();
        }

        public async Task<object> GetChannelStatsAsync(ChannelType channel, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var attempts = await _deliveryAttemptRepository.GetQueryable()
                .Where(a => a.Channel == channel && 
                           a.AttemptedAt >= startDate && 
                           a.AttemptedAt <= endDate)
                .ToListAsync();

            return new
            {
                Channel = channel.ToString(),
                TotalAttempts = attempts.Count,
                SuccessfulAttempts = attempts.Count(a => a.Success),
                FailedAttempts = attempts.Count(a => !a.Success),
                SuccessRate = attempts.Any() ? (decimal)attempts.Count(a => a.Success) / attempts.Count * 100 : 0,
                AverageResponseTimeMs = attempts.Any() ? attempts.Average(a => a.ResponseTimeMs) : 0,
                TotalCost = attempts.Where(a => a.CostAmount.HasValue).Sum(a => a.CostAmount.Value),
                FallbackCount = attempts.Count(a => a.FallbackReason.HasValue),
                Period = new { StartDate = startDate, EndDate = endDate }
            };
        }

        public async Task<object> GetOverallStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var attempts = await _deliveryAttemptRepository.GetQueryable()
                .Where(a => a.AttemptedAt >= startDate && a.AttemptedAt <= endDate)
                .ToListAsync();

            return new
            {
                TotalAttempts = attempts.Count,
                SuccessfulAttempts = attempts.Count(a => a.Success),
                FailedAttempts = attempts.Count(a => !a.Success),
                SuccessRate = attempts.Any() ? (decimal)attempts.Count(a => a.Success) / attempts.Count * 100 : 0,
                AverageResponseTimeMs = attempts.Any() ? attempts.Average(a => a.ResponseTimeMs) : 0,
                TotalCost = attempts.Where(a => a.CostAmount.HasValue).Sum(a => a.CostAmount.Value),
                FallbackCount = attempts.Count(a => a.FallbackReason.HasValue),
                ByChannel = attempts.GroupBy(a => a.Channel).Select(g => new
                {
                    Channel = g.Key.ToString(),
                    TotalAttempts = g.Count(),
                    SuccessfulAttempts = g.Count(a => a.Success),
                    SuccessRate = (decimal)g.Count(a => a.Success) / g.Count() * 100
                }),
                Period = new { StartDate = startDate, EndDate = endDate }
            };
        }
    }
}
