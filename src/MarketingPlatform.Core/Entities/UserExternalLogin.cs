namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Maps external provider users to internal ApplicationUser
    /// </summary>
    public class UserExternalLogin : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int ProviderId { get; set; }
        public string ProviderUserId { get; set; } = string.Empty; // User ID from external provider
        public string ProviderUserName { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? AccessToken { get; set; } // Encrypted in production
        public string? RefreshToken { get; set; } // Encrypted in production
        public DateTime? TokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ExternalAuthProvider Provider { get; set; } = null!;
    }
}
