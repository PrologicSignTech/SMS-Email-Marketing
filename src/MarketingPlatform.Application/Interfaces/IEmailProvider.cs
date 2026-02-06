namespace MarketingPlatform.Application.Interfaces
{
    public interface IEmailProvider
    {
        Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendEmailAsync(
            string recipient, 
            string subject, 
            string textBody, 
            string? htmlBody = null);
        
        Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId);
    }
}
