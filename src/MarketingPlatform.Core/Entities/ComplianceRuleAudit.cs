using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Audit trail for compliance rule changes
    /// </summary>
    public class ComplianceRuleAudit : BaseEntity
    {
        /// <summary>
        /// Reference to the compliance rule
        /// </summary>
        public int ComplianceRuleId { get; set; }

        /// <summary>
        /// Action performed on the rule
        /// </summary>
        public ComplianceAuditAction Action { get; set; }

        /// <summary>
        /// User who performed the action
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// Previous state of the rule (JSON)
        /// </summary>
        public string? PreviousState { get; set; }

        /// <summary>
        /// New state of the rule (JSON)
        /// </summary>
        public string? NewState { get; set; }

        /// <summary>
        /// Reason for the change
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// IP address of the user who made the change
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Additional metadata (JSON)
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Navigation property to the compliance rule
        /// </summary>
        public virtual ComplianceRule ComplianceRule { get; set; } = null!;
    }
}
