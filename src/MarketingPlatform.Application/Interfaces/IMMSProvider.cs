namespace MarketingPlatform.Application.Interfaces
{
    public interface IMMSProvider
    {
        Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendMMSAsync(
            string recipient, 
            string message, 
            List<string> mediaUrls);
        
        Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId);
    }
}
