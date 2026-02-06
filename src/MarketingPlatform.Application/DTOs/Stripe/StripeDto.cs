namespace MarketingPlatform.Application.DTOs.Stripe
{
    public class StripeProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
    }

    public class StripePriceDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Interval { get; set; } = "month";
    }

    public class StripeWebhookEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public object? Data { get; set; }
    }
}
