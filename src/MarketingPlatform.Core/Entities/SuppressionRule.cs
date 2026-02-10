using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Auto-suppression rule engine entity.
    /// Defines rules that automatically suppress contacts when triggered by events.
    /// </summary>
    public class SuppressionRule : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SuppressionTrigger Trigger { get; set; }
        public SuppressionScope Scope { get; set; } = SuppressionScope.Global;
        public SuppressionChannel Channel { get; set; } = SuppressionChannel.All;
        public SuppressionType SuppressionType { get; set; } = SuppressionType.OptOut;
        public bool IsActive { get; set; } = true;
        public bool IsSystemRule { get; set; } = false;
        public int Priority { get; set; } = 0;
        public string? AutoReason { get; set; }
        public int TriggerCount { get; set; } = 0;
        public DateTime? LastTriggeredAt { get; set; }

        // Navigation
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
