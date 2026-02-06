using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.SuppressionList
{
    public class SuppressionListDto
    {
        public int Id { get; set; }
        public string PhoneOrEmail { get; set; } = string.Empty;
        public SuppressionType Type { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
