using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    public class MessageStatusUpdateDto
    {
        public string ExternalMessageId { get; set; } = string.Empty;
        public MessageStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? CostAmount { get; set; }
    }
}
