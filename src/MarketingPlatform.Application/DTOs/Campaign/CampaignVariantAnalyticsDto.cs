namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignVariantAnalyticsDto
    {
        public int Id { get; set; }
        public int CampaignVariantId { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public int TotalClicks { get; set; }
        public int TotalOptOuts { get; set; }
        public int TotalBounces { get; set; }
        public int TotalOpens { get; set; }
        public int TotalReplies { get; set; }
        public int TotalConversions { get; set; }
        
        public decimal DeliveryRate { get; set; }
        public decimal ClickRate { get; set; }
        public decimal OptOutRate { get; set; }
        public decimal OpenRate { get; set; }
        public decimal BounceRate { get; set; }
        public decimal ConversionRate { get; set; }
        
        public decimal? ConfidenceLevel { get; set; }
        public bool? IsStatisticallySignificant { get; set; }
    }
}
