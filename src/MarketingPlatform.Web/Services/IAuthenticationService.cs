using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// Service for handling authentication operations with the API
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate user and create a session
    /// </summary>
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);

    /// <summary>
    /// Log out the current user
    /// </summary>
    Task<bool> LogoutAsync(string userId);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto request);
}
