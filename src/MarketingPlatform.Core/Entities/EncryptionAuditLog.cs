namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Audit log for encryption operations and key rotation events.
    /// Tracks all encryption/decryption activities for compliance and security audits.
    /// </summary>
    public class EncryptionAuditLog : BaseEntity
    {
        public string Operation { get; set; } = string.Empty; // Encrypt, Decrypt, KeyRotation
        public string EntityType { get; set; } = string.Empty; // Contact, MessageTemplate, etc.
        public int? EntityId { get; set; }
        public string? FieldName { get; set; }
        public string KeyVersion { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? IpAddress { get; set; }
        public DateTime OperationTimestamp { get; set; }
    }
}
