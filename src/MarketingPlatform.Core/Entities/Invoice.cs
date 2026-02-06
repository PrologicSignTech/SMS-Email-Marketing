using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class Invoice : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int? UserSubscriptionId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceStatus Status { get; set; }
        public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Stripe;
        public decimal Amount { get; set; }
        public decimal Tax { get; set; } = 0;
        public decimal Total { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        
        // Stripe Integration
        public string? StripeInvoiceId { get; set; }
        
        // PayPal Integration
        public string? PayPalInvoiceId { get; set; }
        
        public string? Description { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual UserSubscription? UserSubscription { get; set; }
        public virtual ICollection<BillingHistory> BillingHistories { get; set; } = new List<BillingHistory>();
    }
}
