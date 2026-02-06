using MarketingPlatform.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarketingPlatform.Infrastructure.Data.Converters
{
    /// <summary>
    /// EF Core value converter that automatically encrypts data when writing to database
    /// and decrypts when reading from database. Uses AES-256 encryption.
    /// </summary>
    public class EncryptedStringConverter : ValueConverter<string, string>
    {
        public EncryptedStringConverter(IEncryptionService encryptionService)
            : base(
                v => encryptionService.Encrypt(v),
                v => encryptionService.Decrypt(v))
        {
        }
    }

    /// <summary>
    /// Value comparer for encrypted strings to ensure EF Core change tracking works correctly.
    /// </summary>
    public class EncryptedStringComparer : Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<string>
    {
        public EncryptedStringComparer()
            : base(
                (l, r) => string.Equals(l, r),
                v => v == null ? 0 : v.GetHashCode(),
                v => v)
        {
        }
    }
}
