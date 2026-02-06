using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class Workflow : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerCriteria { get; set; } // JSON
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public virtual ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
    }
}
