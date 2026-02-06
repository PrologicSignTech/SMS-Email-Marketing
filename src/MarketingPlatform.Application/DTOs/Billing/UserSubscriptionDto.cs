using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Billing
{
    public class UserSubscriptionDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public PaymentProvider PaymentProvider { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsYearly { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? PayPalSubscriptionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
