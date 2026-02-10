using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.SuppressionRule
{
    public class SuppressionRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SuppressionTrigger Trigger { get; set; }
        public SuppressionScope Scope { get; set; }
        public SuppressionChannel Channel { get; set; }
        public SuppressionType SuppressionType { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemRule { get; set; }
        public int Priority { get; set; }
        public string? AutoReason { get; set; }
        public int TriggerCount { get; set; }
        public DateTime? LastTriggeredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSuppressionRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SuppressionTrigger Trigger { get; set; }
        public SuppressionScope Scope { get; set; } = SuppressionScope.Global;
        public SuppressionChannel Channel { get; set; } = SuppressionChannel.All;
        public SuppressionType SuppressionType { get; set; } = SuppressionType.OptOut;
        public int Priority { get; set; } = 0;
        public string? AutoReason { get; set; }
    }

    public class UpdateSuppressionRuleDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public SuppressionScope? Scope { get; set; }
        public SuppressionChannel? Channel { get; set; }
        public SuppressionType? SuppressionType { get; set; }
        public bool? IsActive { get; set; }
        public int? Priority { get; set; }
        public string? AutoReason { get; set; }
    }
}
