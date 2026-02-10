using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.PhoneNumber
{
    public class PhoneNumberDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string? FriendlyName { get; set; }
        public PhoneNumberType NumberType { get; set; }
        public PhoneNumberCapability Capabilities { get; set; }
        public PhoneNumberStatus Status { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePhoneNumberDto
    {
        public string Number { get; set; } = string.Empty;
        public string? FriendlyName { get; set; }
        public PhoneNumberType NumberType { get; set; } = PhoneNumberType.Local;
        public PhoneNumberCapability Capabilities { get; set; } = PhoneNumberCapability.SMS;
        public decimal MonthlyRate { get; set; }
        public string? Country { get; set; } = "US";
        public string? Region { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePhoneNumberDto
    {
        public string? FriendlyName { get; set; }
        public PhoneNumberCapability? Capabilities { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignPhoneNumberDto
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class PurchasePhoneNumberDto
    {
        public string Number { get; set; } = string.Empty;
        public string? FriendlyName { get; set; }
        public PhoneNumberType NumberType { get; set; } = PhoneNumberType.Local;
        public PhoneNumberCapability Capabilities { get; set; } = PhoneNumberCapability.SMS;
        public string? Country { get; set; } = "US";
        public string? Region { get; set; }
    }
}
