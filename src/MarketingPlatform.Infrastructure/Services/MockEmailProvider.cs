using MarketingPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Services
{
    public class MockEmailProvider : IEmailProvider
    {
        private readonly ILogger<MockEmailProvider> _logger;

        public MockEmailProvider(ILogger<MockEmailProvider> logger)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendEmailAsync(
            string recipient, string subject, string textBody, string? htmlBody = null)
        {
            await Task.Delay(100);

            var random = new Random();
            var success = random.Next(100) < 97; // 97% success for Email

            if (success)
            {
                var externalId = $"EMAIL_{Guid.NewGuid():N}";
                var cost = 0.001m; // $0.001 per email
                
                _logger.LogInformation("Mock Email sent to {Recipient}. Subject: {Subject}, ExternalId: {ExternalId}", 
                    recipient, subject, externalId);
                
                return (true, externalId, null, cost);
            }

            _logger.LogWarning("Mock Email failed to {Recipient}", recipient);
            return (false, null, "Mock email delivery failure", null);
        }

        public async Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId)
        {
            await Task.Delay(50);
            
            var random = new Random();
            var delivered = random.Next(100) < 99;
            
            return delivered 
                ? (true, null) 
                : (false, "Email bounced");
        }
    }
}
