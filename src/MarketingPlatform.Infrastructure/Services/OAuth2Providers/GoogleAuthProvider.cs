using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Infrastructure.Services.OAuth2Providers
{
    public class GoogleAuthProvider : IOAuth2Provider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GoogleAuthProvider> _logger;

        public string ProviderName => "Google";

        public GoogleAuthProvider(IHttpClientFactory httpClientFactory, ILogger<GoogleAuthProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string BuildAuthorizationUrl(ExternalAuthProvider config, string redirectUri, string state)
        {
            var scopes = Uri.EscapeDataString(config.Scopes);
            
            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={Uri.EscapeDataString(config.ClientId)}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                   $"scope={scopes}&" +
                   $"state={Uri.EscapeDataString(state)}&" +
                   $"access_type=offline&" +
                   $"prompt=consent";
        }

        public async Task<OAuth2TokenResponseDto> ExchangeCodeForTokenAsync(ExternalAuthProvider config, string code, string redirectUri)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            return await PostTokenRequestAsync(tokenEndpoint, requestBody);
        }

        public async Task<ExternalUserInfoDto> GetUserInfoAsync(ExternalAuthProvider config, string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);

            return new ExternalUserInfoDto
            {
                ProviderId = ProviderName,
                ProviderUserId = userInfo.GetProperty("id").GetString() ?? string.Empty,
                Email = userInfo.GetProperty("email").GetString() ?? string.Empty,
                FirstName = userInfo.TryGetProperty("given_name", out var given) ? given.GetString() : null,
                LastName = userInfo.TryGetProperty("family_name", out var family) ? family.GetString() : null,
                DisplayName = userInfo.TryGetProperty("name", out var name) ? name.GetString() : null,
                Picture = userInfo.TryGetProperty("picture", out var picture) ? picture.GetString() : null
            };
        }

        public async Task<OAuth2TokenResponseDto> RefreshTokenAsync(ExternalAuthProvider config, string refreshToken)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            return await PostTokenRequestAsync(tokenEndpoint, requestBody);
        }

        public async Task<bool> RevokeTokenAsync(ExternalAuthProvider config, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync($"https://oauth2.googleapis.com/revoke?token={token}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully revoked Google token");
                    return true;
                }
                
                _logger.LogWarning("Failed to revoke Google token: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking Google token");
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
                _logger.LogError("Google token request failed: {StatusCode} - {Content}", response.StatusCode, content);
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
