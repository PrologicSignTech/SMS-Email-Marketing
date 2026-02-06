using MarketingPlatform.Application.DTOs.Message;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IWebhookService
    {
        Task<bool> ProcessMessageStatusUpdateAsync(string externalMessageId, string status, string? errorMessage = null);
        Task<bool> ProcessInboundMessageAsync(string from, string to, string body, string? externalId = null);
        Task<bool> ProcessOptOutAsync(string phoneNumber, string? source = null);
        Task<bool> ProcessDeliveryStatusAsync(string externalMessageId, DeliveryStatusDto statusDto);
        bool ValidateWebhookSignature(string signature, string payload, string secret);
    }
}
