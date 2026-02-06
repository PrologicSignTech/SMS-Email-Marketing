using MarketingPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Services
{
    public class MockMMSProvider : IMMSProvider
    {
        private readonly ILogger<MockMMSProvider> _logger;

        public MockMMSProvider(ILogger<MockMMSProvider> logger)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string? ExternalId, string? Error, decimal? Cost)> SendMMSAsync(
            string recipient, string message, List<string> mediaUrls)
        {
            await Task.Delay(150);

            var random = new Random();
            var success = random.Next(100) < 93; // 93% success for MMS

            if (success)
            {
                var externalId = $"MMS_{Guid.NewGuid():N}";
                var cost = CalculateMMSCost(message, mediaUrls.Count);
                
                _logger.LogInformation("Mock MMS sent to {Recipient} with {MediaCount} media. ExternalId: {ExternalId}", 
                    recipient, mediaUrls.Count, externalId);
                
                return (true, externalId, null, cost);
            }

            _logger.LogWarning("Mock MMS failed to {Recipient}", recipient);
            return (false, null, "Mock MMS delivery failure", null);
        }

        public async Task<(bool Success, string? Error)> GetDeliveryStatusAsync(string externalMessageId)
        {
            await Task.Delay(50);
            
            var random = new Random();
            var delivered = random.Next(100) < 97;
            
            return delivered 
                ? (true, null) 
                : (false, "MMS delivery failed");
        }

        private decimal CalculateMMSCost(string message, int mediaCount)
        {
            // Base cost: $0.02 per MMS + $0.01 per media file
            var baseCost = 0.02m;
            var mediaCost = mediaCount * 0.01m;
            return baseCost + mediaCost;
        }
    }
}
