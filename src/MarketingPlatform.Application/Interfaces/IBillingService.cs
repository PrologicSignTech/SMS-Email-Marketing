using MarketingPlatform.Application.DTOs.Billing;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IBillingService
    {
        // Subscription Lifecycle
        Task<UserSubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto);
        Task<UserSubscriptionDto?> GetUserSubscriptionAsync(string userId);
        Task<UserSubscriptionDto?> UpgradeSubscriptionAsync(string userId, SubscriptionUpgradeDto dto);
        Task<UserSubscriptionDto?> DowngradeSubscriptionAsync(string userId, int newPlanId);
        Task<bool> CancelSubscriptionAsync(string userId, string reason);
        Task<bool> ReactivateSubscriptionAsync(string userId);
        Task<bool> RenewSubscriptionAsync(string userId);
        
        // Invoice Management
        Task<InvoiceDto> CreateInvoiceAsync(string userId, decimal amount, string description, PaymentProvider provider);
        Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId);
        Task<List<InvoiceDto>> GetUserInvoicesAsync(string userId);
        Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, string? externalInvoiceId, PaymentProvider provider);
        Task<bool> MarkInvoiceAsFailedAsync(int invoiceId);
        
        // Payment Failure Handling
        Task<bool> HandlePaymentFailureAsync(string userId, int invoiceId);
        Task<bool> RetryPaymentAsync(string userId, int invoiceId);
        Task<List<InvoiceDto>> GetFailedPaymentsAsync(string userId);
        
        // Billing History
        Task<BillingHistoryDto> RecordTransactionAsync(string userId, TransactionType type, decimal amount, string description, PaymentProvider provider);
        Task<List<BillingHistoryDto>> GetBillingHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
