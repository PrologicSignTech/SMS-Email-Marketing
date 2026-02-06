using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Billing
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceStatus Status { get; set; }
        public PaymentProvider PaymentProvider { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? StripeInvoiceId { get; set; }
        public string? PayPalInvoiceId { get; set; }
    }
}
