using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class WorkflowExecution : BaseEntity
    {
        public int WorkflowId { get; set; }
        public int ContactId { get; set; }
        public WorkflowExecutionStatus Status { get; set; }
        public int CurrentStepOrder { get; set; } = 0;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual Workflow Workflow { get; set; } = null!;
    }
}
