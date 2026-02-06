namespace MarketingPlatform.Core.Entities
{
    public class CampaignVariantAnalytics : BaseEntity
    {
        public int CampaignVariantId { get; set; }
        public int TotalSent { get; set; } = 0;
        public int TotalDelivered { get; set; } = 0;
        public int TotalFailed { get; set; } = 0;
        public int TotalClicks { get; set; } = 0;
        public int TotalOptOuts { get; set; } = 0;
        public int TotalBounces { get; set; } = 0;
        public int TotalOpens { get; set; } = 0;
        public int TotalReplies { get; set; } = 0;
        public int TotalConversions { get; set; } = 0;
        
        public decimal DeliveryRate { get; set; } = 0;
        public decimal ClickRate { get; set; } = 0;
        public decimal OptOutRate { get; set; } = 0;
        public decimal OpenRate { get; set; } = 0;
        public decimal BounceRate { get; set; } = 0;
        public decimal ConversionRate { get; set; } = 0;
        
        // Statistical significance
        public decimal? ConfidenceLevel { get; set; }
        public bool? IsStatisticallySignificant { get; set; }
        
        // Navigation properties
        public virtual CampaignVariant CampaignVariant { get; set; } = null!;
    }
}
