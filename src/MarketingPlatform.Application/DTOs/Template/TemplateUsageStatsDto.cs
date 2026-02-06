namespace MarketingPlatform.Application.DTOs.Template
{
    public class TemplateUsageStatsDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public int TotalCampaigns { get; set; }
        public int TotalMessages { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
