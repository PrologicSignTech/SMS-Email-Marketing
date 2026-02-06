using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// DTO for journey execution status and monitoring
    /// </summary>
    public class JourneyExecutionDto
    {
        public int Id { get; set; }
        public int JourneyId { get; set; }
        public string JourneyName { get; set; } = string.Empty;
        public int ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public WorkflowExecutionStatus Status { get; set; }
        public int CurrentStepOrder { get; set; }
        public string? CurrentStepName { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
