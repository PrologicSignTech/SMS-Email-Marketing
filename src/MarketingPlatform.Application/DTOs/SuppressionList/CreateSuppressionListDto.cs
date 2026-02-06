using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.SuppressionList
{
    public class CreateSuppressionListDto
    {
        public string PhoneOrEmail { get; set; } = string.Empty;
        public SuppressionType Type { get; set; }
        public string? Reason { get; set; }
    }
}
