namespace MarketingPlatform.Application.DTOs.Auth
{
    public class OAuth2LoginRequestDto
    {
        public string ProviderName { get; set; } = string.Empty; // AzureAD, Google, Okta, Cognito
        public string? RedirectUri { get; set; }
        public string? State { get; set; } // CSRF protection
    }

    public class OAuth2CallbackDto
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
    }

    public class OAuth2TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string? IdToken { get; set; }
        public string? Scope { get; set; }
    }

    public class ExternalUserInfoDto
    {
        public string ProviderId { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? Picture { get; set; }
        public Dictionary<string, string> AdditionalClaims { get; set; } = new();
    }

    public class ExternalAuthProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CreateExternalAuthProviderDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string? TenantId { get; set; }
        public string? Domain { get; set; }
        public string? Region { get; set; }
        public string? UserPoolId { get; set; }
        public string CallbackPath { get; set; } = "/signin-oauth";
        public string Scopes { get; set; } = "openid profile email";
        public bool IsEnabled { get; set; } = true;
        public string? ConfigurationJson { get; set; }
    }

    public class UpdateExternalAuthProviderDto
    {
        public string? DisplayName { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Authority { get; set; }
        public string? TenantId { get; set; }
        public string? Domain { get; set; }
        public string? Region { get; set; }
        public string? UserPoolId { get; set; }
        public string? CallbackPath { get; set; }
        public string? Scopes { get; set; }
        public bool? IsEnabled { get; set; }
        public string? ConfigurationJson { get; set; }
    }
}
