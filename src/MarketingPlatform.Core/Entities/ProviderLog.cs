namespace MarketingPlatform.Core.Entities
{
    public class ProviderLog : BaseEntity
    {
        public int MessageProviderId { get; set; }
        public string RequestPayload { get; set; } = string.Empty;
        public string? ResponsePayload { get; set; }
        public int? StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual MessageProvider MessageProvider { get; set; } = null!;
    }
}
