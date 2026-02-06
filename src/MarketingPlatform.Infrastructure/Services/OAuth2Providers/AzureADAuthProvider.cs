using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Infrastructure.Services.OAuth2Providers
{
    public class AzureADAuthProvider : IOAuth2Provider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AzureADAuthProvider> _logger;

        public string ProviderName => "AzureAD";

        public AzureADAuthProvider(IHttpClientFactory httpClientFactory, ILogger<AzureADAuthProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string BuildAuthorizationUrl(ExternalAuthProvider config, string redirectUri, string state)
        {
            var tenantId = config.TenantId ?? "common";
            var authority = config.Authority.TrimEnd('/');
            var scopes = Uri.EscapeDataString(config.Scopes);
            
            return $"{authority}/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={Uri.EscapeDataString(config.ClientId)}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                   $"response_mode=query&" +
                   $"scope={scopes}&" +
                   $"state={Uri.EscapeDataString(state)}";
        }

        public async Task<OAuth2TokenResponseDto> ExchangeCodeForTokenAsync(ExternalAuthProvider config, string code, string redirectUri)
        {
            var tenantId = config.TenantId ?? "common";
            var authority = config.Authority.TrimEnd('/');
            var tokenEndpoint = $"{authority}/{tenantId}/oauth2/v2.0/token";

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["scope"] = config.Scopes
            };

            return await PostTokenRequestAsync(tokenEndpoint, requestBody);
        }

        public async Task<ExternalUserInfoDto> GetUserInfoAsync(ExternalAuthProvider config, string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);

            return new ExternalUserInfoDto
            {
                ProviderId = ProviderName,
                ProviderUserId = userInfo.GetProperty("id").GetString() ?? string.Empty,
                Email = userInfo.TryGetProperty("mail", out var mail) ? mail.GetString() ?? userInfo.GetProperty("userPrincipalName").GetString() ?? string.Empty : string.Empty,
                FirstName = userInfo.TryGetProperty("givenName", out var given) ? given.GetString() : null,
                LastName = userInfo.TryGetProperty("surname", out var surname) ? surname.GetString() : null,
                DisplayName = userInfo.TryGetProperty("displayName", out var display) ? display.GetString() : null
            };
        }

        public async Task<OAuth2TokenResponseDto> RefreshTokenAsync(ExternalAuthProvider config, string refreshToken)
        {
            var tenantId = config.TenantId ?? "common";
            var authority = config.Authority.TrimEnd('/');
            var tokenEndpoint = $"{authority}/{tenantId}/oauth2/v2.0/token";

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token",
                ["scope"] = config.Scopes
            };

            return await PostTokenRequestAsync(tokenEndpoint, requestBody);
        }

        public async Task<bool> RevokeTokenAsync(ExternalAuthProvider config, string token)
        {
            try
            {
                // Azure AD doesn't have a direct revoke endpoint, tokens expire naturally
                // For logout, we'd typically redirect to the logout endpoint
                _logger.LogInformation("Token revocation for Azure AD - tokens will expire naturally");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking Azure AD token");
                return false;
            }
        }

        private async Task<OAuth2TokenResponseDto> PostTokenRequestAsync(string tokenEndpoint, Dictionary<string, string> requestBody)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Azure AD token request failed: {StatusCode} - {Content}", response.StatusCode, content);
                throw new Exception($"Token request failed: {content}");
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuth2TokenResponseDto
            {
                AccessToken = tokenResponse.GetProperty("access_token").GetString() ?? string.Empty,
                RefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                TokenType = tokenResponse.TryGetProperty("token_type", out var tt) ? tt.GetString() ?? "Bearer" : "Bearer",
                ExpiresIn = tokenResponse.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600,
                IdToken = tokenResponse.TryGetProperty("id_token", out var id) ? id.GetString() : null,
                Scope = tokenResponse.TryGetProperty("scope", out var scope) ? scope.GetString() : null
            };
        }
    }
}
