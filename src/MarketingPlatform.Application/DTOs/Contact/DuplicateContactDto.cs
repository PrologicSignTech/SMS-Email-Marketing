namespace MarketingPlatform.Application.DTOs.Contact
{
    public class DuplicateContactDto
    {
        public int ContactId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DuplicateReason { get; set; } = string.Empty; // "Email", "PhoneNumber", "Both"
    }

    public class CheckDuplicateDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class DuplicateCheckResultDto
    {
        public bool HasDuplicates { get; set; }
        public List<DuplicateContactDto> Duplicates { get; set; } = new();
    }

    public class DuplicateReportDto
    {
        public int TotalDuplicates { get; set; }
        public List<DuplicateGroupDto> DuplicateGroups { get; set; } = new();
    }

    public class DuplicateGroupDto
    {
        public string DuplicateKey { get; set; } = string.Empty; // Email or PhoneNumber value
        public string DuplicateType { get; set; } = string.Empty; // "Email" or "PhoneNumber"
        public int Count { get; set; }
        public List<DuplicateContactDto> Contacts { get; set; } = new();
    }

    public class ResolveDuplicateDto
    {
        public int PrimaryContactId { get; set; }
        public List<int> DuplicateContactIds { get; set; } = new();
        public ResolutionAction Action { get; set; }
    }

    public enum ResolutionAction
    {
        KeepPrimary,    // Keep primary, delete others
        MergeIntoPrimary, // Merge all data into primary, delete others
        KeepAll         // Keep all (mark as resolved/ignored)
    }

    public class ResolveDuplicateResultDto
    {
        public bool Success { get; set; }
        public int ContactsAffected { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
