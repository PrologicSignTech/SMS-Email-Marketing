namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Stores file storage provider configuration
    /// </summary>
    public class FileStorageSettings : BaseEntity
    {
        public string ProviderName { get; set; } = "Local"; // Local, Azure, S3
        public string? ConnectionString { get; set; } // For Azure
        public string? ContainerName { get; set; } // For Azure
        public string? BucketName { get; set; } // For S3
        public string? Region { get; set; } // For S3
        public string? AccessKey { get; set; } // For S3, encrypted
        public string? SecretKey { get; set; } // For S3, encrypted
        public string? LocalBasePath { get; set; } // For Local storage
        public bool IsEnabled { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? ConfigurationJson { get; set; } // Additional provider-specific config
    }
}
