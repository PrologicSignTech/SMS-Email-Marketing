namespace MarketingPlatform.Application.DTOs.Integration
{
    public enum CRMType
    {
        Salesforce,
        HubSpot,
        Zoho,
        MicrosoftDynamics,
        Custom
    }

    public enum SyncDirection
    {
        FromCRM,
        ToCRM,
        Bidirectional
    }

    public class CRMSyncConfigDto
    {
        public CRMType CRMType { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string? InstanceUrl { get; set; }
        public SyncDirection Direction { get; set; }
        public Dictionary<string, string> FieldMappings { get; set; } = new();
        public bool AutoSync { get; set; }
        public int SyncIntervalMinutes { get; set; } = 60;
    }

    public class CRMSyncResultDto
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CRMConnectionTestResultDto
    {
        public bool IsConnected { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Version { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CRMFieldDto
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsCustom { get; set; }
    }

    public class CRMContactDto
    {
        public string ExternalId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public Dictionary<string, object> CustomFields { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
