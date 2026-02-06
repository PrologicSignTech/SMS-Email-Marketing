namespace MarketingPlatform.Application.DTOs.PayPal
{
    public class PayPalProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "SERVICE";
        public string Category { get; set; } = "SOFTWARE";
    }

    public class PayPalPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "ACTIVE";
        public string BillingCycle { get; set; } = "MONTH";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class PayPalWebhookEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public object? Resource { get; set; }
    }
}
