namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordAnalyticsDto
    {
        public int KeywordId { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        
        // Usage Statistics
        public int TotalResponses { get; set; }
        public int UniqueContacts { get; set; }
        public int RepeatUsageCount { get; set; }
        
        // Opt-In Statistics
        public int TotalOptIns { get; set; }
        public int SuccessfulOptIns { get; set; }
        public int FailedOptIns { get; set; }
        public decimal OptInConversionRate { get; set; }
        
        // Response Statistics
        public int ResponsesSent { get; set; }
        public int ResponsesFailed { get; set; }
        public decimal ResponseSuccessRate { get; set; }
        
        // Campaign Integration
        public int? LinkedCampaignId { get; set; }
        public string? LinkedCampaignName { get; set; }
        public int CampaignRelatedActivities { get; set; }
        
        // Time-based Analytics
        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public int ActivitiesLast24Hours { get; set; }
        public int ActivitiesLast7Days { get; set; }
        public int ActivitiesLast30Days { get; set; }
    }
}
