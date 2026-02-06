using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class WorkflowStep : BaseEntity
    {
        public int WorkflowId { get; set; }
        public int StepOrder { get; set; }
        public WorkflowActionType ActionType { get; set; }
        public string? ActionConfiguration { get; set; } // JSON
        public int? DelayMinutes { get; set; }
        
        // Visual Designer Properties
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public string? NodeLabel { get; set; }
        
        // Branch Logic for conditional workflows
        public string? BranchCondition { get; set; } // JSON condition expression
        public int? NextNodeOnTrue { get; set; } // Step ID for true branch
        public int? NextNodeOnFalse { get; set; } // Step ID for false branch

        // Navigation properties
        public virtual Workflow Workflow { get; set; } = null!;
    }
}
