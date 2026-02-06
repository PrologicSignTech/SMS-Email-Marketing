namespace MarketingPlatform.Application.DTOs.Message
{
    public class MessageDeliveryReportDto
    {
        public int CampaignId { get; set; }
        public int TotalMessages { get; set; }
        public int Queued { get; set; }
        public int Sending { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Failed { get; set; }
        public int Bounced { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal FailureRate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
    }
}
