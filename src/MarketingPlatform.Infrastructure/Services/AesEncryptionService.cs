using System.Security.Cryptography;
using System.Text;
using MarketingPlatform.Core.Interfaces;

namespace MarketingPlatform.Infrastructure.Services
{
    /// <summary>
    /// AES-256 encryption service for protecting sensitive data at rest.
    /// Uses AES with CBC mode and PKCS7 padding for secure encryption.
    /// All keys are managed through IKeyManagementService.
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly IKeyManagementService _keyManagementService;

        public AesEncryptionService(IKeyManagementService keyManagementService)
        {
            _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        }

        /// <summary>
        /// Encrypts plain text using AES-256-CBC encryption.
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            return EncryptAsync(plainText).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Decrypts encrypted text using AES-256-CBC encryption.
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            return DecryptAsync(encryptedText).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Encrypts plain text asynchronously using AES-256-CBC encryption.
        /// Format: {keyVersion}:{IV}:{EncryptedData}
        /// </summary>
        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                var key = await _keyManagementService.GetEncryptionKeyAsync();
                var keyVersion = await _keyManagementService.GetCurrentKeyVersionAsync();

                using var aes = Aes.Create();
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                var iv = Convert.ToBase64String(aes.IV);
                var encrypted = Convert.ToBase64String(msEncrypt.ToArray());

                // Include key version for key rotation support
                return $"{keyVersion}:{iv}:{encrypted}";
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Failed to encrypt data", ex);
            }
        }

        /// <summary>
        /// Decrypts encrypted text asynchronously using AES-256-CBC encryption.
        /// Supports multiple key versions for key rotation.
        /// </summary>
        public async Task<string> DecryptAsync(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                var parts = encryptedText.Split(':');
                if (parts.Length != 3)
                    throw new FormatException("Invalid encrypted data format. Expected format: version:IV:data");

                var keyVersion = parts[0];
                byte[] iv;
                byte[] encrypted;

                try
                {
                    iv = Convert.FromBase64String(parts[1]);
                    encrypted = Convert.FromBase64String(parts[2]);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Invalid Base64 encoding in encrypted data", ex);
                }

                // Retrieve the appropriate key version for decryption
                byte[] key;
                try
                {
                    key = await _keyManagementService.GetEncryptionKeyAsync(keyVersion);
                }
                catch (Exception ex)
                {
                    throw new CryptographicException($"Failed to retrieve encryption key version '{keyVersion}'", ex);
                }

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(encrypted);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (CryptographicException)
            {
                // Re-throw cryptographic exceptions as-is for proper error handling
                throw;
            }
            catch (FormatException)
            {
                // Re-throw format exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Unexpected error during decryption", ex);
            }
        }
    }
}
