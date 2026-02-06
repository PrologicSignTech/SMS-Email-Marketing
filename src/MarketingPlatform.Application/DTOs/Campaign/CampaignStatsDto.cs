namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignStatsDto
    {
        public int TotalSent { get; set; }
        public int Delivered { get; set; }
        public int Failed { get; set; }
        public int Bounced { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal FailureRate { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
