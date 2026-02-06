namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Stores configuration for external OAuth2/SSO authentication providers
    /// </summary>
    public class ExternalAuthProvider : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "AzureAD", "Google", "Okta", "Cognito"
        public string DisplayName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty; // OAuth2, SAML, OpenIDConnect
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty; // Encrypted in production
        public string Authority { get; set; } = string.Empty; // Token endpoint URL
        public string? TenantId { get; set; } // For Azure AD multi-tenant support (future use)
        public string? Domain { get; set; } // For Okta, Auth0, etc.
        public string? Region { get; set; } // For AWS Cognito
        public string? UserPoolId { get; set; } // For AWS Cognito
        public string CallbackPath { get; set; } = "/signin-oauth";
        public string Scopes { get; set; } = "openid profile email"; // Space-separated scopes
        public bool IsEnabled { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? ConfigurationJson { get; set; } // For additional provider-specific config
        
        // Navigation properties
        public virtual ICollection<UserExternalLogin> UserExternalLogins { get; set; } = new List<UserExternalLogin>();
    }
}
