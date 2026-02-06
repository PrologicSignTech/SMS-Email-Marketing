using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace MarketingPlatform.API.Middleware
{
    /// <summary>
    /// Middleware for API rate limiting and throttling
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
        {
            // Skip rate limiting for certain endpoints
            if (ShouldSkipRateLimiting(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                // Allow unauthenticated requests to pass through for authentication endpoints
                await _next(context);
                return;
            }

            var tenantId = context.User?.FindFirst("TenantId")?.Value; // Assuming tenant claim exists
            var endpoint = context.Request.Path.Value ?? "";
            var httpMethod = context.Request.Method;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                // Check rate limit
                var rateLimitStatus = await rateLimitService.CheckApiRateLimitAsync(userId, tenantId, endpoint, httpMethod);

                if (rateLimitStatus.IsLimited)
                {
                    // Rate limit exceeded - add headers before setting status code
                    context.Response.Headers["X-RateLimit-Limit"] = rateLimitStatus.MaxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitStatus.WindowResetTime).ToUnixTimeSeconds().ToString();
                    context.Response.Headers["Retry-After"] = rateLimitStatus.RetryAfterSeconds?.ToString() ?? "60";
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "application/json";

                    // Log the violation
                    await rateLimitService.LogRateLimitViolationAsync(
                        userId,
                        tenantId,
                        endpoint,
                        httpMethod,
                        ipAddress,
                        $"{rateLimitStatus.MaxRequests} requests per {rateLimitStatus.TimeWindowSeconds} seconds",
                        rateLimitStatus.MaxRequests + 1, // Current attempt
                        rateLimitStatus.MaxRequests,
                        rateLimitStatus.TimeWindowSeconds,
                        rateLimitStatus.RetryAfterSeconds ?? 60
                    );

                    var errorResponse = new
                    {
                        error = "Rate limit exceeded",
                        message = $"You have exceeded the rate limit of {rateLimitStatus.MaxRequests} requests per {rateLimitStatus.TimeWindowSeconds} seconds for this endpoint.",
                        retryAfter = rateLimitStatus.RetryAfterSeconds,
                        resetTime = rateLimitStatus.WindowResetTime,
                        endpoint = endpoint
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                    return;
                }

                // Record the request
                await rateLimitService.RecordApiRequestAsync(userId, tenantId, endpoint, httpMethod);

                // Add rate limit headers to successful requests
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-RateLimit-Limit"] = rateLimitStatus.MaxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = rateLimitStatus.RemainingRequests.ToString();
                    context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitStatus.WindowResetTime).ToUnixTimeSeconds().ToString();
                    return Task.CompletedTask;
                });

                // Continue to next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware for user {UserId}, endpoint {Endpoint}", userId, endpoint);
                // Continue even if rate limiting fails - don't block legitimate requests
                await _next(context);
            }
        }

        private bool ShouldSkipRateLimiting(PathString path)
        {
            // Skip rate limiting for these endpoints
            var skipPaths = new[]
            {
                "/health",
                "/swagger",
                "/hangfire",
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh-token"
            };

            return skipPaths.Any(skip => path.StartsWithSegments(skip, StringComparison.OrdinalIgnoreCase));
        }
    }
}
