namespace MarketingPlatform.Core.Entities
{
    public class CampaignAnalytics : BaseEntity
    {
        public int CampaignId { get; set; }
        public int TotalSent { get; set; } = 0;
        public int TotalDelivered { get; set; } = 0;
        public int TotalFailed { get; set; } = 0;
        public int TotalClicks { get; set; } = 0;
        public int TotalOptOuts { get; set; } = 0;
        public decimal DeliveryRate { get; set; } = 0;
        public decimal ClickRate { get; set; } = 0;
        public decimal OptOutRate { get; set; } = 0;

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}
