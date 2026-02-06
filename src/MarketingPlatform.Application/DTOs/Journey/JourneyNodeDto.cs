using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// Represents a node/step in the journey workflow designer
    /// </summary>
    public class JourneyNodeDto
    {
        public int Id { get; set; }
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
