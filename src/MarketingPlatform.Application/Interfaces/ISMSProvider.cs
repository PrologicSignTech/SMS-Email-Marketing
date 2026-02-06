namespace MarketingPlatform.Application.Interfaces
{
    public interface ISMSProvider
    {
        Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendSMSAsync(
            string recipient, 
            string message);
        
        Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId);
    }
}
