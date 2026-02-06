namespace MarketingPlatform.Core.Interfaces
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data using AES-256 encryption.
    /// All encryption keys should be stored in a secure key vault.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the given plain text using AES-256 encryption.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted text</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts the given encrypted text using AES-256 encryption.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted text</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string encryptedText);

        /// <summary>
        /// Encrypts the given plain text asynchronously using AES-256 encryption.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted text</returns>
        Task<string> EncryptAsync(string plainText);

        /// <summary>
        /// Decrypts the given encrypted text asynchronously using AES-256 encryption.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted text</param>
        /// <returns>Decrypted plain text</returns>
        Task<string> DecryptAsync(string encryptedText);
    }
}
