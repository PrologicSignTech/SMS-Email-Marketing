namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordAssignmentDto
    {
        public int Id { get; set; }
        public int KeywordId { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        public int CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty;
        public string? AssignedByUserName { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateKeywordAssignmentDto
    {
        public int KeywordId { get; set; }
        public int CampaignId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateKeywordAssignmentDto
    {
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
}
