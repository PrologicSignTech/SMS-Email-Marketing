namespace MarketingPlatform.Application.DTOs.Audience
{
    public class AudienceSegmentDto
    {
        public int TotalContacts { get; set; }
        public List<int> ContactIds { get; set; } = new List<int>();
    }
}
