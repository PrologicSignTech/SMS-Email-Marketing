using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Models
{
    public class GroupRule
    {
        public RuleField Field { get; set; }
        public RuleOperator Operator { get; set; }
        public string? Value { get; set; }
        public string? AttributeName { get; set; } // For CustomAttribute field type
    }
}
