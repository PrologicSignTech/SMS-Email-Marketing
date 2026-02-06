using MarketingPlatform.Application.DTOs.PayPal;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPayPalService
    {
        // Product Management
        Task<PayPalProductDto> CreateProductAsync(string name, string? description);
        Task<PayPalProductDto> UpdateProductAsync(string productId, string name, string? description);
        Task<bool> DeleteProductAsync(string productId);
        
        // Plan Management
        Task<PayPalPlanDto> CreatePlanAsync(string productId, string name, decimal amount, string billingCycle);
        Task<PayPalPlanDto> UpdatePlanAsync(string planId, bool active);
        Task<List<PayPalPlanDto>> GetProductPlansAsync(string productId);
        
        // Plan Synchronization
        Task<bool> SyncPlanToPayPalAsync(int planId);
        Task<bool> SyncAllPlansToPayPalAsync();
        
        // Webhook Handling
        Task<bool> HandleWebhookEventAsync(string json, string signature);
        Task ProcessBillingSubscriptionActivatedAsync(string subscriptionId);
        Task ProcessBillingSubscriptionCancelledAsync(string subscriptionId);
        Task ProcessPaymentSaleCompletedAsync(string saleId);
        Task ProcessPaymentSaleRefundedAsync(string saleId);
    }
}
