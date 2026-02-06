using System.Security.Cryptography;
using MarketingPlatform.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MarketingPlatform.Infrastructure.Services
{
    /// <summary>
    /// Key management service that retrieves encryption keys from configuration.
    /// In production, this should be replaced with Azure Key Vault or AWS KMS integration.
    /// Supports key versioning for rotation scenarios.
    /// </summary>
    public class ConfigurationKeyManagementService : IKeyManagementService
    {
        private readonly IConfiguration _configuration;
        private const string KeySectionPath = "Encryption:Keys";
        private const string CurrentVersionPath = "Encryption:CurrentKeyVersion";

        public ConfigurationKeyManagementService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the current active encryption key.
        /// </summary>
        public async Task<byte[]> GetEncryptionKeyAsync()
        {
            var currentVersion = await GetCurrentKeyVersionAsync();
            return await GetEncryptionKeyAsync(currentVersion);
        }

        /// <summary>
        /// Gets a specific version of the encryption key.
        /// This enables decryption of data encrypted with older keys after rotation.
        /// </summary>
        public Task<byte[]> GetEncryptionKeyAsync(string keyVersion)
        {
            var keyPath = $"{KeySectionPath}:{keyVersion}";
            var keyBase64 = _configuration[keyPath];

            if (string.IsNullOrEmpty(keyBase64))
            {
                throw new InvalidOperationException($"Encryption key version '{keyVersion}' not found in configuration. " +
                    "Please ensure the encryption key is properly configured in appsettings or Key Vault.");
            }

            try
            {
                var key = Convert.FromBase64String(keyBase64);
                
                // Validate key length for AES-256 (32 bytes)
                if (key.Length != 32)
                {
                    throw new InvalidOperationException($"Encryption key must be 32 bytes (256 bits) for AES-256. " +
                        $"Current key is {key.Length} bytes.");
                }

                return Task.FromResult(key);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Encryption key is not in valid Base64 format.", ex);
            }
        }

        /// <summary>
        /// Gets the current active key version identifier.
        /// </summary>
        public Task<string> GetCurrentKeyVersionAsync()
        {
            var version = _configuration[CurrentVersionPath];
            
            if (string.IsNullOrEmpty(version))
            {
                // Default to v1 for backward compatibility
                return Task.FromResult("v1");
            }

            return Task.FromResult(version);
        }

        /// <summary>
        /// Rotates the encryption key by creating a new version.
        /// Note: Configuration-based keys require manual rotation.
        /// For automatic rotation, use Azure Key Vault or AWS KMS.
        /// </summary>
        public Task<string> RotateKeyAsync()
        {
            // Configuration-based rotation requires manual intervention
            // Return a task with informative exception rather than throwing synchronously
            return Task.FromException<string>(new NotSupportedException(
                "Key rotation with configuration-based storage requires manual intervention. " +
                "In production, use Azure Key Vault or AWS KMS for automatic key rotation. " +
                "To rotate keys manually: " +
                "1. Generate a new 256-bit key using: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) " +
                "2. Add the new key to configuration with a new version (e.g., v2) " +
                "3. Update Encryption:CurrentKeyVersion to the new version " +
                "4. Old keys must remain in configuration to decrypt existing data"));
        }

        /// <summary>
        /// Generates a new random 256-bit encryption key.
        /// This is a utility method for generating keys during setup.
        /// </summary>
        public static string GenerateNewKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[32]; // 256 bits
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }
}
