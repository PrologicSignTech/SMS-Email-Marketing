using MarketingPlatform.Application.DTOs.Stripe;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IStripeService
    {
        // Product Management
        Task<StripeProductDto> CreateProductAsync(string name, string? description);
        Task<StripeProductDto> UpdateProductAsync(string productId, string name, string? description);
        Task<bool> DeleteProductAsync(string productId);
        
        // Price Management
        Task<StripePriceDto> CreatePriceAsync(string productId, decimal amount, string interval);
        Task<StripePriceDto> UpdatePriceAsync(string priceId, bool active);
        Task<List<StripePriceDto>> GetProductPricesAsync(string productId);
        
        // Plan Synchronization
        Task<bool> SyncPlanToStripeAsync(int planId);
        Task<bool> SyncAllPlansToStripeAsync();
        
        // Webhook Handling
        Task<bool> HandleWebhookEventAsync(string json, string signature);
        Task ProcessInvoicePaidAsync(string invoiceId);
        Task ProcessInvoicePaymentFailedAsync(string invoiceId);
        Task ProcessSubscriptionUpdatedAsync(string subscriptionId);
        Task ProcessSubscriptionDeletedAsync(string subscriptionId);
    }
}
