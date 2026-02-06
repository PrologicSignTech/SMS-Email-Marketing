namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class ContactEngagementHistoryDto
    {
        public int ContactId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // Engagement Statistics
        public int TotalMessagesSent { get; set; }
        public int TotalMessagesDelivered { get; set; }
        public int TotalClicks { get; set; }
        public DateTime? LastEngagementDate { get; set; }
        public decimal EngagementScore { get; set; }

        // Campaign Participation
        public int CampaignsParticipated { get; set; }
        public List<CampaignParticipationDto> CampaignHistory { get; set; } = new();

        // Engagement Timeline
        public List<EngagementEventDto> EngagementEvents { get; set; } = new();
    }

    public class CampaignParticipationDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public DateTime ParticipatedAt { get; set; }
        public int MessagesReceived { get; set; }
        public int Clicks { get; set; }
        public bool OptedOut { get; set; }
    }

    public class EngagementEventDto
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty; // "MessageSent", "MessageDelivered", "Click", "OptOut"
        public string? CampaignName { get; set; }
        public string? Details { get; set; }
    }
}
