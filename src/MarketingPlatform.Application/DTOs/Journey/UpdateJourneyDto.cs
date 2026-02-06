using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// DTO for updating an existing journey/workflow
    /// </summary>
    public class UpdateJourneyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerCriteria { get; set; }
        public bool IsActive { get; set; }
        
        public List<CreateJourneyNodeDto> Nodes { get; set; } = new List<CreateJourneyNodeDto>();
    }
}
