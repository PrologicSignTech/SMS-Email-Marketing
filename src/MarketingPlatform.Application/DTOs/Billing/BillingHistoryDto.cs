using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Billing
{
    public class BillingHistoryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public PaymentProvider PaymentProvider { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
