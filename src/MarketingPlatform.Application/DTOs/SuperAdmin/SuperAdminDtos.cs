using System.ComponentModel.DataAnnotations;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.SuperAdmin
{
    /// <summary>
    /// Super Admin Role read model
    /// </summary>
    public class SuperAdminRoleDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
        public string? AssignedByEmail { get; set; }
        public string AssignmentReason { get; set; } = string.Empty;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public string? RevokedByEmail { get; set; }
        public string? RevocationReason { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Request to assign super admin privileges
    /// </summary>
    public class AssignSuperAdminDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string AssignmentReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to revoke super admin privileges
    /// </summary>
    public class RevokeSuperAdminDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string RevocationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Privileged Action Log read model
    /// </summary>
    public class PrivilegedActionLogDto
    {
        public int Id { get; set; }
        public PrivilegedActionType ActionType { get; set; }
        public string ActionTypeName { get; set; } = string.Empty;
        public ActionSeverity Severity { get; set; }
        public string SeverityName { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string PerformedByEmail { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? EntityName { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public string? BeforeState { get; set; }
        public string? AfterState { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestPath { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Filter criteria for privileged action logs
    /// </summary>
    public class PrivilegedActionFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PrivilegedActionType? ActionType { get; set; }
        public string? UserId { get; set; }
        public ActionSeverity? Severity { get; set; }
        public string? EntityType { get; set; }
    }

    /// <summary>
    /// Platform Configuration read model
    /// </summary>
    public class PlatformConfigurationDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ConfigurationCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; }
        public bool IsActive { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? LastModifiedByEmail { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request to update a configuration
    /// </summary>
    public class UpdateConfigurationDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to create a privileged action log
    /// </summary>
    public class CreatePrivilegedActionLogDto
    {
        [Required]
        public PrivilegedActionType ActionType { get; set; }

        [Required]
        public ActionSeverity Severity { get; set; }

        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? EntityName { get; set; }

        [Required]
        [StringLength(2000)]
        public string ActionDescription { get; set; } = string.Empty;

        public string? BeforeState { get; set; }
        public string? AfterState { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public string? Metadata { get; set; }
    }
}
