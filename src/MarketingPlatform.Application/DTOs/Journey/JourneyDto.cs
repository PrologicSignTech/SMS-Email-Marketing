using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// Complete journey/workflow with all nodes
    /// </summary>
    public class JourneyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerCriteria { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public List<JourneyNodeDto> Nodes { get; set; } = new List<JourneyNodeDto>();
        
        // Statistics
        public int TotalExecutions { get; set; }
        public int ActiveExecutions { get; set; }
        public int CompletedExecutions { get; set; }
        public int FailedExecutions { get; set; }
    }
}
