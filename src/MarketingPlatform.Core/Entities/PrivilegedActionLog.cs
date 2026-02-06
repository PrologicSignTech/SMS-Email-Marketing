using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Comprehensive audit log for all privileged actions performed by super admins
    /// </summary>
    public class PrivilegedActionLog : BaseEntity
    {
        /// <summary>
        /// Type of privileged action performed
        /// </summary>
        public PrivilegedActionType ActionType { get; set; }

        /// <summary>
        /// Severity level of the action
        /// </summary>
        public ActionSeverity Severity { get; set; }

        /// <summary>
        /// User ID of the person who performed this action
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// Type of entity affected by this action (e.g., "User", "Role", "Configuration")
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of the entity affected by this action
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// Name or identifier of the entity for display purposes
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// Human-readable description of the action
        /// </summary>
        public string ActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// JSON representation of the entity state before the action
        /// </summary>
        public string? BeforeState { get; set; }

        /// <summary>
        /// JSON representation of the entity state after the action
        /// </summary>
        public string? AfterState { get; set; }

        /// <summary>
        /// IP address from which the action was performed
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// User agent of the client performing the action
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request path/endpoint that triggered this action
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// Whether the action completed successfully
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the action failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When the action was performed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata in JSON format
        /// </summary>
        public string? Metadata { get; set; }

        // Navigation properties
        public virtual ApplicationUser PerformedByUser { get; set; } = null!;
    }
}
