namespace MarketingPlatform.Application.DTOs.Keyword
{
    public class KeywordActivityDto
    {
        public int Id { get; set; }
        public int KeywordId { get; set; }
        public string KeywordText { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IncomingMessage { get; set; } = string.Empty;
        public string? ResponseSent { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
