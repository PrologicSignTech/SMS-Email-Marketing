namespace MarketingPlatform.Application.DTOs.Journey
{
    /// <summary>
    /// Statistics for a journey/workflow
    /// </summary>
    public class JourneyStatsDto
    {
        public int JourneyId { get; set; }
        public string JourneyName { get; set; } = string.Empty;
        public int TotalExecutions { get; set; }
        public int ActiveExecutions { get; set; }
        public int CompletedExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int PausedExecutions { get; set; }
        public double CompletionRate { get; set; }
        public double FailureRate { get; set; }
        public double AverageExecutionTimeMinutes { get; set; }
    }
}
