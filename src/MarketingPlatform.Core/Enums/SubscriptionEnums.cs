namespace MarketingPlatform.Core.Enums
{
    public enum SubscriptionStatus
    {
        Trial,
        Active,
        PastDue,
        Canceled,
        Expired
    }

    public enum InvoiceStatus
    {
        Draft,
        Open,
        Paid,
        Uncollectible,
        Void
    }

    public enum TransactionType
    {
        Subscription,
        Overage,
        Refund,
        Credit
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum PaymentProvider
    {
        Stripe,
        PayPal
    }
}
