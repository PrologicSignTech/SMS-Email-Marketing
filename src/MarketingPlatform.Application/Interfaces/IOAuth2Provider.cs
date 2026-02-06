using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    /// <summary>
    /// Base interface for OAuth2/SSO provider implementations
    /// </summary>
    public interface IOAuth2Provider
    {
        string ProviderName { get; }
        
        /// <summary>
        /// Build OAuth2 authorization URL
        /// </summary>
        string BuildAuthorizationUrl(ExternalAuthProvider config, string redirectUri, string state);
        
        /// <summary>
        /// Exchange authorization code for access token
        /// </summary>
        Task<OAuth2TokenResponseDto> ExchangeCodeForTokenAsync(ExternalAuthProvider config, string code, string redirectUri);
        
        /// <summary>
        /// Get user information from provider
        /// </summary>
        Task<ExternalUserInfoDto> GetUserInfoAsync(ExternalAuthProvider config, string accessToken);
        
        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        Task<OAuth2TokenResponseDto> RefreshTokenAsync(ExternalAuthProvider config, string refreshToken);
        
        /// <summary>
        /// Revoke access token
        /// </summary>
        Task<bool> RevokeTokenAsync(ExternalAuthProvider config, string token);
    }
}
