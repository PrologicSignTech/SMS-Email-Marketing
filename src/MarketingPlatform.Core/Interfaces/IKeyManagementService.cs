namespace MarketingPlatform.Core.Interfaces
{
    /// <summary>
    /// Service for managing encryption keys using Azure Key Vault or similar secure storage.
    /// Supports key rotation and secure key retrieval.
    /// </summary>
    public interface IKeyManagementService
    {
        /// <summary>
        /// Retrieves the current encryption key from secure storage.
        /// </summary>
        /// <returns>The encryption key as byte array</returns>
        Task<byte[]> GetEncryptionKeyAsync();

        /// <summary>
        /// Retrieves a specific version of the encryption key for decryption of old data.
        /// </summary>
        /// <param name="keyVersion">The version identifier of the key</param>
        /// <returns>The encryption key as byte array</returns>
        Task<byte[]> GetEncryptionKeyAsync(string keyVersion);

        /// <summary>
        /// Rotates the encryption key by creating a new version.
        /// </summary>
        /// <returns>The new key version identifier</returns>
        Task<string> RotateKeyAsync();

        /// <summary>
        /// Gets the current active key version identifier.
        /// </summary>
        /// <returns>The current key version</returns>
        Task<string> GetCurrentKeyVersionAsync();
    }
}
