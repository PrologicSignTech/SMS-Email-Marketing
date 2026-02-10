using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class PhoneNumber : BaseEntity
    {
        public string Number { get; set; } = string.Empty;
        public string? FriendlyName { get; set; }
        public PhoneNumberType NumberType { get; set; } = PhoneNumberType.Local;
        public PhoneNumberCapability Capabilities { get; set; } = PhoneNumberCapability.SMS;
        public PhoneNumberStatus Status { get; set; } = PhoneNumberStatus.Available;
        public string? AssignedToUserId { get; set; }
        public string? PurchasedByUserId { get; set; }
        public string? ProviderId { get; set; }
        public string? ProviderNumberSid { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? Country { get; set; } = "US";
        public string? Region { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Notes { get; set; }
    }
}
