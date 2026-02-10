using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MarketingPlatform.Web.Middleware;

/// <summary>
/// Middleware that automatically refreshes expired JWT tokens using the refresh token
/// stored in the authentication cookie. This prevents "Session expired" errors when
/// the JWT token expires before the cookie session.
/// </summary>
public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tokenExpirationClaim = context.User.FindFirst("token_expiration")?.Value;
            var accessToken = context.User.FindFirst("access_token")?.Value;
            var refreshToken = context.User.FindFirst("refresh_token")?.Value;

            if (!string.IsNullOrEmpty(tokenExpirationClaim) &&
                !string.IsNullOrEmpty(accessToken) &&
                !string.IsNullOrEmpty(refreshToken))
            {
                if (DateTime.TryParse(tokenExpirationClaim, null, System.Globalization.DateTimeStyles.RoundtripKind, out var tokenExpiration))
                {
                    // Refresh if token expires within the next 5 minutes
                    var timeUntilExpiry = tokenExpiration - DateTime.UtcNow;

                    if (timeUntilExpiry.TotalMinutes <= 5)
                    {
                        _logger.LogInformation("JWT token expires in {Minutes:F1} minutes. Attempting refresh...",
                            timeUntilExpiry.TotalMinutes);

                        try
                        {
                            var authService = context.RequestServices.GetRequiredService<Web.Services.IAuthenticationService>();
                            var apiClient = context.RequestServices.GetRequiredService<IApiClient>();

                            // Need to set the current (possibly expired) token first for the refresh call
                            // The refresh endpoint may not require auth, but set it just in case
                            apiClient.ClearAuthorizationToken();

                            var refreshResult = await authService.RefreshTokenAsync(new RefreshTokenRequestDto
                            {
                                Token = accessToken,
                                RefreshToken = refreshToken
                            });

                            if (refreshResult?.Success == true && refreshResult.Data != null)
                            {
                                _logger.LogInformation("JWT token refreshed successfully. New expiration: {Expiry}",
                                    refreshResult.Data.TokenExpiration);

                                // The AuthenticationService.RefreshTokenAsync already:
                                // 1. Creates new auth cookie with updated claims
                                // 2. Sets the new token on the ApiClient
                                // So we're good - the new token will be used for this request
                            }
                            else
                            {
                                _logger.LogWarning("Token refresh failed: {Message}",
                                    refreshResult?.Message ?? "Unknown error");

                                // If refresh fails, sign the user out to force re-login
                                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                                context.Response.Redirect("/auth/login?expired=true");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error during automatic token refresh");

                            // Don't block the request if refresh fails with an exception
                            // The controller will get a 401 from the API and the user will see the error
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to add the TokenRefreshMiddleware to the pipeline
/// </summary>
public static class TokenRefreshMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenRefreshMiddleware>();
    }
}
