namespace MarketingPlatform.Application.DTOs.Message
{
    public class DeliveryStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public decimal? Cost { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
