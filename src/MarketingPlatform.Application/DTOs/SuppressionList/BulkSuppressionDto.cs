using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.SuppressionList
{
    public class BulkSuppressionDto
    {
        public List<string> PhoneOrEmails { get; set; } = new List<string>();
        public SuppressionType Type { get; set; }
        public string? Reason { get; set; }
    }
}
