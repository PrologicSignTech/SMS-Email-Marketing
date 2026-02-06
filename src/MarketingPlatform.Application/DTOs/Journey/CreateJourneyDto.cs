using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// DTO for creating a new journey/workflow
    /// </summary>
    public class CreateJourneyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerCriteria { get; set; }
        public bool IsActive { get; set; } = false;
        
        public List<CreateJourneyNodeDto> Nodes { get; set; } = new List<CreateJourneyNodeDto>();
    }
    
    /// <summary>
    /// DTO for creating a journey node
    /// </summary>
    public class CreateJourneyNodeDto
    {
        public int StepOrder { get; set; }
        public WorkflowActionType ActionType { get; set; }
        public string? ActionConfiguration { get; set; }
        public int? DelayMinutes { get; set; }
        
        // Visual Designer Properties
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public string? NodeLabel { get; set; }
        
        // Branch Logic
        public string? BranchCondition { get; set; }
        public int? NextNodeOnTrue { get; set; }
        public int? NextNodeOnFalse { get; set; }
    }
}
