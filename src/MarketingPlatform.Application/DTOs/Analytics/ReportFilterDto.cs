namespace MarketingPlatform.Application.DTOs.Analytics
{
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CampaignId { get; set; }
        public int? ContactId { get; set; }
        public string? Channel { get; set; }
        public string? Status { get; set; }
    }
}
