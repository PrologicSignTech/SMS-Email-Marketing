using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.DTOs.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// Implementation of authentication service that communicates with the API
/// and manages cookie-based authentication
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IApiClient apiClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthenticationService> logger)
    {
        _apiClient = apiClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // Call API to authenticate
            var response = await _apiClient.PostAsync<LoginRequestDto, ApiResponse<AuthResponseDto>>(
                "/api/auth/login",
                request);

            if (response?.Success == true && response.Data != null)
            {
                // Create authentication cookie
                await CreateAuthenticationCookieAsync(response.Data);

                // Set API client token for subsequent requests
                _apiClient.SetAuthorizationToken(response.Data.Token);

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
            }

            return response ?? ApiResponse<AuthResponseDto>.ErrorResponse("Login failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return ApiResponse<AuthResponseDto>.ErrorResponse($"Login error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var response = await _apiClient.PostAsync<RegisterRequestDto, ApiResponse<AuthResponseDto>>(
                "/api/auth/register",
                request);

            if (response?.Success == true && response.Data != null)
            {
                // Create authentication cookie
                await CreateAuthenticationCookieAsync(response.Data);

                // Set API client token
                _apiClient.SetAuthorizationToken(response.Data.Token);

                _logger.LogInformation("User {Email} registered successfully", request.Email);
            }

            return response ?? ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return ApiResponse<AuthResponseDto>.ErrorResponse($"Registration error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _apiClient.PostAsync<RefreshTokenRequestDto, ApiResponse<AuthResponseDto>>(
                "/api/auth/refresh-token",
                request);

            if (response?.Success == true && response.Data != null)
            {
                // Update authentication cookie
                await CreateAuthenticationCookieAsync(response.Data);

                // Update API client token
                _apiClient.SetAuthorizationToken(response.Data.Token);

                _logger.LogInformation("Token refreshed successfully");
            }

            return response ?? ApiResponse<AuthResponseDto>.ErrorResponse("Token refresh failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return ApiResponse<AuthResponseDto>.ErrorResponse($"Token refresh error: {ex.Message}");
        }
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        try
        {
            // Set authorization for this request
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User.Identity?.IsAuthenticated == true)
            {
                var token = httpContext.User.FindFirst("access_token")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    _apiClient.SetAuthorizationToken(token);
                }
            }

            // Call API logout
            await _apiClient.PostAsync<object>("/auth/logout", new { });

            // Clear authentication cookie
            if (httpContext != null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            // Clear API client token
            _apiClient.ClearAuthorizationToken();

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto request)
    {
        try
        {
            var response = await _apiClient.PostAsync<ChangePasswordDto, ApiResponse<bool>>(
                "/api/auth/change-password",
                request);

            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user {UserId}", userId);
            return false;
        }
    }

    private async Task CreateAuthenticationCookieAsync(AuthResponseDto authResponse)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authResponse.UserId),
            new Claim(ClaimTypes.Email, authResponse.Email),
            new Claim(ClaimTypes.Name, $"{authResponse.FirstName} {authResponse.LastName}"),
            new Claim("access_token", authResponse.Token),
            new Claim("refresh_token", authResponse.RefreshToken),
            new Claim("token_expiration", authResponse.TokenExpiration.ToString("o")),
            new Claim("Permissions", authResponse.Permissions.ToString())
        };

        // Add roles
        if (authResponse.Roles != null)
        {
            foreach (var role in authResponse.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // Remember me
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8), // Cookie session lasts 8 hours (independent of JWT expiry)
            AllowRefresh = true
        };

        _logger.LogInformation("Creating authentication cookie for user {UserId}", authResponse.UserId);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            authProperties);

        _logger.LogInformation("SignInAsync completed. User should now be authenticated.");

        // Verify the user is authenticated
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("Cookie set successfully - User is authenticated");
        }
        else
        {
            _logger.LogWarning("Cookie may not have been set - User is NOT authenticated after SignInAsync");
        }
    }
}
