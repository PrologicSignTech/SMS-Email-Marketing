using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces;
using MarketingPlatform.Core.Interfaces.Repositories;

using System.Security.Cryptography;

namespace MarketingPlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Encrypted repository for Contact entities.
    /// Automatically encrypts sensitive fields (Email, PhoneNumber, CustomAttributes) at rest.
    /// </summary>
    public class ContactEncryptedRepository : EncryptedRepository<Contact>
    {
        private readonly IEncryptionService _encryptionService;

        public ContactEncryptedRepository(
            IRepository<Contact> innerRepository,
            IEncryptionService encryptionService,
            IServiceProvider serviceProvider)
            : base(innerRepository, encryptionService, serviceProvider)
        {
            _encryptionService = encryptionService;
        }

        protected override void EncryptEntity(Contact contact)
        {
            if (contact == null) return;

            // Encrypt sensitive PII fields
            if (!string.IsNullOrEmpty(contact.Email) && !IsEncrypted(contact.Email))
            {
                contact.Email = _encryptionService.Encrypt(contact.Email);
            }

            if (!string.IsNullOrEmpty(contact.PhoneNumber) && !IsEncrypted(contact.PhoneNumber))
            {
                contact.PhoneNumber = _encryptionService.Encrypt(contact.PhoneNumber);
            }

            if (!string.IsNullOrEmpty(contact.CustomAttributes) && !IsEncrypted(contact.CustomAttributes))
            {
                contact.CustomAttributes = _encryptionService.Encrypt(contact.CustomAttributes);
            }
        }

        protected override void DecryptEntity(Contact contact)
        {
            if (contact == null) return;

            // Decrypt sensitive PII fields
            if (!string.IsNullOrEmpty(contact.Email) && IsEncrypted(contact.Email))
            {
                try
                {
                    contact.Email = _encryptionService.Decrypt(contact.Email);
                }
                catch (CryptographicException ex)
                {
                    // If decryption fails, field might not be encrypted (legacy data)
                    // Log warning for investigation but don't fail the entire operation
                    System.Diagnostics.Debug.WriteLine($"Failed to decrypt email for contact {contact.Id}: {ex.Message}");
                    // Leave as is - may be legacy unencrypted data
                }
                catch (FormatException ex)
                {
                    // Invalid base64 format - likely legacy data
                    System.Diagnostics.Debug.WriteLine($"Invalid encrypted format for email in contact {contact.Id}: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(contact.PhoneNumber) && IsEncrypted(contact.PhoneNumber))
            {
                try
                {
                    contact.PhoneNumber = _encryptionService.Decrypt(contact.PhoneNumber);
                }
                catch (CryptographicException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to decrypt phone for contact {contact.Id}: {ex.Message}");
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid encrypted format for phone in contact {contact.Id}: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(contact.CustomAttributes) && IsEncrypted(contact.CustomAttributes))
            {
                try
                {
                    contact.CustomAttributes = _encryptionService.Decrypt(contact.CustomAttributes);
                }
                catch (CryptographicException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to decrypt custom attributes for contact {contact.Id}: {ex.Message}");
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid encrypted format for custom attributes in contact {contact.Id}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if a value is already encrypted by looking for the version:IV:data format.
        /// Validates the structure more thoroughly to avoid false positives.
        /// </summary>
        private bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Check for encryption format: version:IV:encryptedData
            var parts = value.Split(':');
            if (parts.Length != 3)
                return false;

            // Version should start with 'v' (e.g., v1, v2)
            if (!parts[0].StartsWith("v"))
                return false;

            // IV and encrypted data should be valid base64
            try
            {
                Convert.FromBase64String(parts[1]); // IV
                Convert.FromBase64String(parts[2]); // Encrypted data
                return true;
            }
            catch (FormatException)
            {
                return false; // Not valid base64, therefore not encrypted
            }
        }
    }
}
