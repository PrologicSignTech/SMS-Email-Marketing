using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents a compliance rule that must be enforced across the platform
    /// </summary>
    public class ComplianceRule : BaseEntity
    {
        /// <summary>
        /// Unique name for the rule
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the compliance rule
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of compliance rule
        /// </summary>
        public ComplianceRuleType RuleType { get; set; }

        /// <summary>
        /// Current status of the rule
        /// </summary>
        public ComplianceRuleStatus Status { get; set; } = ComplianceRuleStatus.Draft;

        /// <summary>
        /// JSON configuration for the rule
        /// </summary>
        public string Configuration { get; set; } = "{}";

        /// <summary>
        /// Priority of the rule (higher = more important)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Whether this rule is mandatory
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// When the rule becomes effective
        /// </summary>
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the rule expires (optional)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Comma-separated list of regions this rule applies to (empty = all)
        /// </summary>
        public string? ApplicableRegions { get; set; }

        /// <summary>
        /// Comma-separated list of services this rule applies to (empty = all)
        /// </summary>
        public string? ApplicableServices { get; set; }

        /// <summary>
        /// User who created this rule
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// User who last modified this rule
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Navigation property for audit trail
        /// </summary>
        public virtual ICollection<ComplianceRuleAudit> AuditTrail { get; set; } = new List<ComplianceRuleAudit>();
    }
}
