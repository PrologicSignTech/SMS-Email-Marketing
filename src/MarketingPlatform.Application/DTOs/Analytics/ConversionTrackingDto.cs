namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class ConversionTrackingDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        
        // Conversion Metrics
        public int TotalRecipients { get; set; }
        public int TotalClicks { get; set; }
        public int TotalConversions { get; set; }
        
        // Conversion Rates
        public decimal ClickThroughRate { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal ClickToConversionRate { get; set; }

        // URL Performance
        public List<UrlConversionDto> UrlConversions { get; set; } = new();

        // Timeline
        public List<ConversionTimelineDto> ConversionTimeline { get; set; } = new();
    }

    public class UrlConversionDto
    {
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int TotalClicks { get; set; }
        public int UniqueClicks { get; set; }
        public DateTime? FirstClickedAt { get; set; }
        public DateTime? LastClickedAt { get; set; }
    }

    public class ConversionTimelineDto
    {
        public DateTime Date { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
