using MarketingPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Services
{
    public class MockSMSProvider : ISMSProvider
    {
        private readonly ILogger<MockSMSProvider> _logger;

        public MockSMSProvider(ILogger<MockSMSProvider> logger)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendSMSAsync(
            string recipient, string message)
        {
            // Simulate API call delay
            await Task.Delay(100);

            // Mock 95% success rate
            var random = new Random();
            var success = random.Next(100) < 95;

            if (success)
            {
                var externalId = $"SMS_{Guid.NewGuid():N}";
                var cost = CalculateSMSCost(message);
                
                _logger.LogInformation("Mock SMS sent to {Recipient}. ExternalId: {ExternalId}", 
                    recipient, externalId);
                
                return (true, externalId, null, cost);
            }

            _logger.LogWarning("Mock SMS failed to {Recipient}", recipient);
            return (false, null, "Mock delivery failure", null);
        }

        public async Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId)
        {
            await Task.Delay(50);
            
            // Mock 98% delivery success
            var random = new Random();
            var delivered = random.Next(100) < 98;
            
            return delivered 
                ? (true, null) 
                : (false, "Message bounced");
        }

        private decimal CalculateSMSCost(string message)
        {
            // $0.0075 per 160 characters
            var segments = Math.Ceiling(message.Length / 160.0);
            return (decimal)(segments * 0.0075);
        }
    }
}
