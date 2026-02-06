using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Billing
{
    public class CreateSubscriptionDto
    {
        public string UserId { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Stripe;
        public bool IsYearly { get; set; } = false;
        public bool StartTrial { get; set; } = false;
        public int TrialDays { get; set; } = 14;
        public string? PaymentMethodId { get; set; }
    }
}
