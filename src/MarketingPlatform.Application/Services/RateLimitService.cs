using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Application.DTOs.RateLimit;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IRepository<FrequencyControl> _frequencyControlRepository;
        private readonly IRepository<ApiRateLimit> _apiRateLimitRepository;
        private readonly IRepository<RateLimitLog> _rateLimitLogRepository;
        private readonly IRepository<ProviderRateLimit> _providerRateLimitRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RateLimitService> _logger;

        public RateLimitService(
            IRepository<FrequencyControl> frequencyControlRepository,
            IRepository<ApiRateLimit> apiRateLimitRepository,
            IRepository<RateLimitLog> rateLimitLogRepository,
            IRepository<ProviderRateLimit> providerRateLimitRepository,
            IUnitOfWork unitOfWork,
            ILogger<RateLimitService> logger)
        {
            _frequencyControlRepository = frequencyControlRepository;
            _apiRateLimitRepository = apiRateLimitRepository;
            _rateLimitLogRepository = rateLimitLogRepository;
            _providerRateLimitRepository = providerRateLimitRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> CanSendMessageAsync(int contactId, string userId)
        {
            var control = await GetOrCreateFrequencyControlAsync(contactId, userId);

            // Reset counters if needed
            await ResetCountersIfNeededAsync(control);

            // Check limits
            if (control.MessagesSentToday >= control.MaxMessagesPerDay)
            {
                _logger.LogWarning("Daily limit reached for contact {ContactId}", contactId);
                return false;
            }

            if (control.MessagesSentThisWeek >= control.MaxMessagesPerWeek)
            {
                _logger.LogWarning("Weekly limit reached for contact {ContactId}", contactId);
                return false;
            }

            if (control.MessagesSentThisMonth >= control.MaxMessagesPerMonth)
            {
                _logger.LogWarning("Monthly limit reached for contact {ContactId}", contactId);
                return false;
            }

            return true;
        }

        public async Task RecordMessageSentAsync(int contactId, string userId)
        {
            var control = await GetOrCreateFrequencyControlAsync(contactId, userId);

            // Reset counters if needed
            await ResetCountersIfNeededAsync(control);

            // Increment counters
            control.MessagesSentToday++;
            control.MessagesSentThisWeek++;
            control.MessagesSentThisMonth++;
            control.LastMessageSentAt = DateTime.UtcNow;
            control.UpdatedAt = DateTime.UtcNow;

            _frequencyControlRepository.Update(control);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Recorded message sent for contact {ContactId}. Daily: {Daily}, Weekly: {Weekly}, Monthly: {Monthly}",
                contactId, control.MessagesSentToday, control.MessagesSentThisWeek, control.MessagesSentThisMonth);
        }

        public async Task<FrequencyControl?> GetFrequencyControlAsync(int contactId, string userId)
        {
            return await _frequencyControlRepository.FirstOrDefaultAsync(fc =>
                fc.ContactId == contactId && fc.UserId == userId && !fc.IsDeleted);
        }

        public async Task UpdateFrequencyControlAsync(int contactId, string userId, int maxPerDay, int maxPerWeek, int maxPerMonth)
        {
            var control = await GetOrCreateFrequencyControlAsync(contactId, userId);

            control.MaxMessagesPerDay = maxPerDay;
            control.MaxMessagesPerWeek = maxPerWeek;
            control.MaxMessagesPerMonth = maxPerMonth;
            control.UpdatedAt = DateTime.UtcNow;

            _frequencyControlRepository.Update(control);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated frequency control for contact {ContactId}. Daily: {Daily}, Weekly: {Weekly}, Monthly: {Monthly}",
                contactId, maxPerDay, maxPerWeek, maxPerMonth);
        }

        public async Task ResetDailyCountersAsync()
        {
            var controls = await _frequencyControlRepository.FindAsync(fc => !fc.IsDeleted);

            foreach (var control in controls)
            {
                control.MessagesSentToday = 0;
                control.UpdatedAt = DateTime.UtcNow;
                _frequencyControlRepository.Update(control);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Reset daily counters for {Count} frequency controls", controls.Count());
        }

        public async Task ResetWeeklyCountersAsync()
        {
            var controls = await _frequencyControlRepository.FindAsync(fc => !fc.IsDeleted);

            foreach (var control in controls)
            {
                control.MessagesSentThisWeek = 0;
                control.UpdatedAt = DateTime.UtcNow;
                _frequencyControlRepository.Update(control);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Reset weekly counters for {Count} frequency controls", controls.Count());
        }

        public async Task ResetMonthlyCountersAsync()
        {
            var controls = await _frequencyControlRepository.FindAsync(fc => !fc.IsDeleted);

            foreach (var control in controls)
            {
                control.MessagesSentThisMonth = 0;
                control.UpdatedAt = DateTime.UtcNow;
                _frequencyControlRepository.Update(control);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Reset monthly counters for {Count} frequency controls", controls.Count());
        }

        private async Task<FrequencyControl> GetOrCreateFrequencyControlAsync(int contactId, string userId)
        {
            var control = await _frequencyControlRepository.FirstOrDefaultAsync(fc =>
                fc.ContactId == contactId && fc.UserId == userId && !fc.IsDeleted);

            if (control == null)
            {
                control = new FrequencyControl
                {
                    ContactId = contactId,
                    UserId = userId,
                    MaxMessagesPerDay = 5,
                    MaxMessagesPerWeek = 20,
                    MaxMessagesPerMonth = 50,
                    MessagesSentToday = 0,
                    MessagesSentThisWeek = 0,
                    MessagesSentThisMonth = 0,
                    LastMessageSentAt = DateTime.UtcNow
                };

                await _frequencyControlRepository.AddAsync(control);
                await _unitOfWork.SaveChangesAsync();
            }

            return control;
        }

        private async Task ResetCountersIfNeededAsync(FrequencyControl control)
        {
            var now = DateTime.UtcNow;
            
            // Handle null LastMessageSentAt for new frequency controls
            if (!control.LastMessageSentAt.HasValue)
            {
                control.LastMessageSentAt = now;
                control.UpdatedAt = DateTime.UtcNow;
                _frequencyControlRepository.Update(control);
                await _unitOfWork.SaveChangesAsync();
                return;
            }
            
            var lastSent = control.LastMessageSentAt.Value;
            bool needsUpdate = false;

            // Reset daily counter if it's a new day
            if (lastSent.Date < now.Date)
            {
                control.MessagesSentToday = 0;
                needsUpdate = true;
            }

            // Reset weekly counter if it's a new week (ISO 8601: week starts on Monday)
            var lastSentWeekStart = GetMondayOfWeek(lastSent);
            var nowWeekStart = GetMondayOfWeek(now);
            if (lastSentWeekStart.Date < nowWeekStart.Date)
            {
                control.MessagesSentThisWeek = 0;
                needsUpdate = true;
            }

            // Reset monthly counter if it's a new month
            if (lastSent.Year < now.Year || (lastSent.Year == now.Year && lastSent.Month < now.Month))
            {
                control.MessagesSentThisMonth = 0;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                control.UpdatedAt = DateTime.UtcNow;
                _frequencyControlRepository.Update(control);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        
        private DateTime GetMondayOfWeek(DateTime date)
        {
            // Handle Sunday (0) as the last day of the week
            int daysToSubtract = ((int)date.DayOfWeek + 6) % 7; // Convert Sunday to 6, Monday to 0, etc.
            return date.AddDays(-daysToSubtract).Date;
        }

        // ===== API Rate Limiting Methods =====

        public async Task<RateLimitStatusDto> CheckApiRateLimitAsync(string userId, string? tenantId, string endpoint, string httpMethod)
        {
            // Get applicable rate limits (user-specific first, then tenant-wide, then default)
            var rateLimits = await GetApplicableRateLimitsAsync(userId, tenantId, endpoint);

            if (!rateLimits.Any())
            {
                // No rate limits configured, return unlimited
                return new RateLimitStatusDto
                {
                    Endpoint = endpoint,
                    MaxRequests = int.MaxValue,
                    RemainingRequests = int.MaxValue,
                    TimeWindowSeconds = 0,
                    WindowResetTime = DateTime.UtcNow,
                    IsLimited = false
                };
            }

            // Use the highest priority (most specific) rate limit
            var rateLimit = rateLimits.First();

            // Reset window if expired
            var now = DateTime.UtcNow;
            if (now >= rateLimit.WindowStartTime.AddSeconds(rateLimit.TimeWindowSeconds))
            {
                rateLimit.CurrentRequestCount = 0;
                rateLimit.WindowStartTime = now;
                rateLimit.UpdatedAt = now;
                _apiRateLimitRepository.Update(rateLimit);
                await _unitOfWork.SaveChangesAsync();
            }

            var remaining = Math.Max(0, rateLimit.MaxRequests - rateLimit.CurrentRequestCount);
            var isLimited = rateLimit.CurrentRequestCount >= rateLimit.MaxRequests;
            var resetTime = rateLimit.WindowStartTime.AddSeconds(rateLimit.TimeWindowSeconds);

            return new RateLimitStatusDto
            {
                Endpoint = endpoint,
                MaxRequests = rateLimit.MaxRequests,
                RemainingRequests = remaining,
                TimeWindowSeconds = rateLimit.TimeWindowSeconds,
                WindowResetTime = resetTime,
                IsLimited = isLimited,
                RetryAfterSeconds = isLimited ? (int)(resetTime - now).TotalSeconds : null
            };
        }

        public async Task RecordApiRequestAsync(string userId, string? tenantId, string endpoint, string httpMethod)
        {
            var rateLimits = await GetApplicableRateLimitsAsync(userId, tenantId, endpoint);

            if (!rateLimits.Any())
                return;

            var rateLimit = rateLimits.First();

            // Reset window if expired
            var now = DateTime.UtcNow;
            if (now >= rateLimit.WindowStartTime.AddSeconds(rateLimit.TimeWindowSeconds))
            {
                rateLimit.CurrentRequestCount = 0;
                rateLimit.WindowStartTime = now;
            }

            rateLimit.CurrentRequestCount++;
            rateLimit.UpdatedAt = now;
            _apiRateLimitRepository.Update(rateLimit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Recorded API request for user {UserId}, endpoint {Endpoint}. Count: {Count}/{Max}",
                userId, endpoint, rateLimit.CurrentRequestCount, rateLimit.MaxRequests);
        }

        public async Task LogRateLimitViolationAsync(string userId, string? tenantId, string endpoint, string httpMethod, 
            string ipAddress, string rateLimitRule, int requestCount, int maxRequests, int timeWindowSeconds, int retryAfterSeconds)
        {
            var log = new RateLimitLog
            {
                UserId = userId,
                TenantId = tenantId,
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                IpAddress = ipAddress,
                RateLimitRule = rateLimitRule,
                RequestCount = requestCount,
                MaxRequests = maxRequests,
                TimeWindowSeconds = timeWindowSeconds,
                TriggeredAt = DateTime.UtcNow,
                RetryAfterSeconds = retryAfterSeconds
            };

            await _rateLimitLogRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("Rate limit violation: User {UserId}, Endpoint {Endpoint}, Method {Method}, IP {IP}, Count: {Count}/{Max}",
                userId, endpoint, httpMethod, ipAddress, requestCount, maxRequests);
        }

        public async Task<List<ApiRateLimitDto>> GetApiRateLimitsAsync(string? userId, string? tenantId)
        {
            var rateLimits = await _apiRateLimitRepository.FindAsync(arl =>
                (string.IsNullOrEmpty(userId) || arl.UserId == userId) &&
                (string.IsNullOrEmpty(tenantId) || arl.TenantId == tenantId) &&
                !arl.IsDeleted);

            return rateLimits.Select(arl => new ApiRateLimitDto
            {
                Id = arl.Id,
                UserId = arl.UserId,
                TenantId = arl.TenantId,
                EndpointPattern = arl.EndpointPattern,
                MaxRequests = arl.MaxRequests,
                TimeWindowSeconds = arl.TimeWindowSeconds,
                IsActive = arl.IsActive,
                Priority = arl.Priority,
                CreatedAt = arl.CreatedAt,
                UpdatedAt = arl.UpdatedAt
            }).ToList();
        }

        public async Task<ApiRateLimitDto> CreateApiRateLimitAsync(CreateApiRateLimitDto dto)
        {
            var rateLimit = new ApiRateLimit
            {
                UserId = dto.UserId,
                TenantId = dto.TenantId,
                EndpointPattern = dto.EndpointPattern,
                MaxRequests = dto.MaxRequests,
                TimeWindowSeconds = dto.TimeWindowSeconds,
                IsActive = dto.IsActive,
                Priority = dto.Priority,
                CurrentRequestCount = 0,
                WindowStartTime = DateTime.UtcNow
            };

            await _apiRateLimitRepository.AddAsync(rateLimit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created API rate limit: {Pattern}, {MaxRequests}/{TimeWindow}s, User: {UserId}, Tenant: {TenantId}",
                dto.EndpointPattern, dto.MaxRequests, dto.TimeWindowSeconds, dto.UserId, dto.TenantId);

            return new ApiRateLimitDto
            {
                Id = rateLimit.Id,
                UserId = rateLimit.UserId,
                TenantId = rateLimit.TenantId,
                EndpointPattern = rateLimit.EndpointPattern,
                MaxRequests = rateLimit.MaxRequests,
                TimeWindowSeconds = rateLimit.TimeWindowSeconds,
                IsActive = rateLimit.IsActive,
                Priority = rateLimit.Priority,
                CreatedAt = rateLimit.CreatedAt,
                UpdatedAt = rateLimit.UpdatedAt
            };
        }

        public async Task<ApiRateLimitDto> UpdateApiRateLimitAsync(int id, UpdateApiRateLimitDto dto)
        {
            var rateLimit = await _apiRateLimitRepository.GetByIdAsync(id);
            if (rateLimit == null)
                throw new KeyNotFoundException($"API rate limit with ID {id} not found");

            rateLimit.MaxRequests = dto.MaxRequests;
            rateLimit.TimeWindowSeconds = dto.TimeWindowSeconds;
            rateLimit.IsActive = dto.IsActive;
            rateLimit.Priority = dto.Priority;
            rateLimit.UpdatedAt = DateTime.UtcNow;

            _apiRateLimitRepository.Update(rateLimit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated API rate limit ID {Id}: {MaxRequests}/{TimeWindow}s",
                id, dto.MaxRequests, dto.TimeWindowSeconds);

            return new ApiRateLimitDto
            {
                Id = rateLimit.Id,
                UserId = rateLimit.UserId,
                TenantId = rateLimit.TenantId,
                EndpointPattern = rateLimit.EndpointPattern,
                MaxRequests = rateLimit.MaxRequests,
                TimeWindowSeconds = rateLimit.TimeWindowSeconds,
                IsActive = rateLimit.IsActive,
                Priority = rateLimit.Priority,
                CreatedAt = rateLimit.CreatedAt,
                UpdatedAt = rateLimit.UpdatedAt
            };
        }

        public async Task DeleteApiRateLimitAsync(int id)
        {
            var rateLimit = await _apiRateLimitRepository.GetByIdAsync(id);
            if (rateLimit == null)
                throw new KeyNotFoundException($"API rate limit with ID {id} not found");

            _apiRateLimitRepository.Remove(rateLimit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted API rate limit ID {Id}", id);
        }

        public async Task<List<RateLimitLogDto>> GetRateLimitLogsAsync(string? userId, DateTime? startDate, DateTime? endDate, int pageSize = 100)
        {
            // Get logs with base filter
            var logs = await _rateLimitLogRepository.FindAsync(log => !log.IsDeleted);
            
            // Apply additional filters in memory (acceptable for paginated results)
            var filteredLogs = logs.AsEnumerable();

            if (!string.IsNullOrEmpty(userId))
                filteredLogs = filteredLogs.Where(log => log.UserId == userId);

            if (startDate.HasValue)
                filteredLogs = filteredLogs.Where(log => log.TriggeredAt >= startDate.Value);

            if (endDate.HasValue)
                filteredLogs = filteredLogs.Where(log => log.TriggeredAt <= endDate.Value);

            var resultLogs = filteredLogs
                .OrderByDescending(log => log.TriggeredAt)
                .Take(pageSize)
                .ToList();

            return resultLogs.Select(log => new RateLimitLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                TenantId = log.TenantId,
                Endpoint = log.Endpoint,
                HttpMethod = log.HttpMethod,
                IpAddress = log.IpAddress,
                RateLimitRule = log.RateLimitRule,
                RequestCount = log.RequestCount,
                MaxRequests = log.MaxRequests,
                TimeWindowSeconds = log.TimeWindowSeconds,
                TriggeredAt = log.TriggeredAt,
                RetryAfterSeconds = log.RetryAfterSeconds
            }).ToList();
        }

        // ===== Provider Rate Limiting Methods =====

        public async Task<bool> CheckProviderRateLimitAsync(string providerName, string providerType, string? userId = null)
        {
            var rateLimit = await GetOrCreateProviderRateLimitAsync(providerName, providerType, userId);

            // Reset window if expired
            var now = DateTime.UtcNow;
            if (now >= rateLimit.WindowStartTime.AddSeconds(rateLimit.TimeWindowSeconds))
            {
                rateLimit.CurrentRequestCount = 0;
                rateLimit.WindowStartTime = now;
                rateLimit.UpdatedAt = now;
                _providerRateLimitRepository.Update(rateLimit);
                await _unitOfWork.SaveChangesAsync();
            }

            if (rateLimit.CurrentRequestCount >= rateLimit.MaxRequests)
            {
                _logger.LogWarning("Provider rate limit exceeded: {Provider} ({Type}), Count: {Count}/{Max}",
                    providerName, providerType, rateLimit.CurrentRequestCount, rateLimit.MaxRequests);
                return false;
            }

            return true;
        }

        public async Task RecordProviderRequestAsync(string providerName, string providerType, string? userId = null)
        {
            var rateLimit = await GetOrCreateProviderRateLimitAsync(providerName, providerType, userId);

            // Reset window if expired
            var now = DateTime.UtcNow;
            if (now >= rateLimit.WindowStartTime.AddSeconds(rateLimit.TimeWindowSeconds))
            {
                rateLimit.CurrentRequestCount = 0;
                rateLimit.WindowStartTime = now;
            }

            rateLimit.CurrentRequestCount++;
            rateLimit.UpdatedAt = now;
            _providerRateLimitRepository.Update(rateLimit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Recorded provider request: {Provider} ({Type}), Count: {Count}/{Max}",
                providerName, providerType, rateLimit.CurrentRequestCount, rateLimit.MaxRequests);
        }

        // ===== Private Helper Methods =====

        private async Task<List<ApiRateLimit>> GetApplicableRateLimitsAsync(string userId, string? tenantId, string endpoint)
        {
            var allLimits = await _apiRateLimitRepository.FindAsync(arl => arl.IsActive && !arl.IsDeleted);

            var applicableLimits = allLimits
                .Where(arl =>
                {
                    // Check if endpoint matches pattern (simple wildcard matching with escaping)
                    try
                    {
                        // Escape the pattern except for wildcards
                        var escapedPattern = System.Text.RegularExpressions.Regex.Escape(arl.EndpointPattern).Replace("\\*", ".*");
                        var regex = new System.Text.RegularExpressions.Regex(
                            $"^{escapedPattern}$",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled,
                            TimeSpan.FromMilliseconds(100)); // Timeout to prevent ReDoS
                        
                        if (!regex.IsMatch(endpoint))
                            return false;
                    }
                    catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
                    {
                        _logger.LogWarning("Regex timeout for pattern {Pattern}", arl.EndpointPattern);
                        return false;
                    }

                    // Match user-specific limits
                    if (!string.IsNullOrEmpty(arl.UserId) && arl.UserId == userId)
                        return true;

                    // Match tenant-specific limits
                    if (!string.IsNullOrEmpty(arl.TenantId) && arl.TenantId == tenantId)
                        return true;

                    // Match global limits (no user or tenant specified)
                    if (string.IsNullOrEmpty(arl.UserId) && string.IsNullOrEmpty(arl.TenantId))
                        return true;

                    return false;
                })
                .OrderByDescending(arl => arl.Priority)
                .ThenBy(arl => string.IsNullOrEmpty(arl.UserId) ? 1 : 0) // User-specific first
                .ThenBy(arl => string.IsNullOrEmpty(arl.TenantId) ? 1 : 0) // Then tenant-specific
                .ToList();

            return applicableLimits;
        }

        private async Task<ProviderRateLimit> GetOrCreateProviderRateLimitAsync(string providerName, string providerType, string? userId)
        {
            var rateLimit = await _providerRateLimitRepository.FirstOrDefaultAsync(prl =>
                prl.ProviderName == providerName &&
                prl.ProviderType == providerType &&
                prl.UserId == userId &&
                !prl.IsDeleted);

            if (rateLimit == null)
            {
                // Default provider limits (can be overridden via configuration)
                var defaultLimits = GetDefaultProviderLimits(providerName, providerType);

                rateLimit = new ProviderRateLimit
                {
                    ProviderName = providerName,
                    ProviderType = providerType,
                    UserId = userId,
                    MaxRequests = defaultLimits.maxRequests,
                    TimeWindowSeconds = defaultLimits.timeWindowSeconds,
                    CurrentRequestCount = 0,
                    WindowStartTime = DateTime.UtcNow,
                    IsActive = true
                };

                await _providerRateLimitRepository.AddAsync(rateLimit);
                await _unitOfWork.SaveChangesAsync();
            }

            return rateLimit;
        }

        private (int maxRequests, int timeWindowSeconds) GetDefaultProviderLimits(string providerName, string providerType)
        {
            // Default provider rate limits - these should ideally be in configuration
            return providerType.ToLower() switch
            {
                "sms" => (100, 60),    // 100 SMS per minute
                "email" => (1000, 60), // 1000 emails per minute
                "mms" => (50, 60),     // 50 MMS per minute
                _ => (100, 60)         // Default: 100 requests per minute
            };
        }
    }
}
