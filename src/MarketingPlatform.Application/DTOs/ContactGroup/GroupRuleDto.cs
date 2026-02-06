using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.ContactGroup
{
    public class GroupRuleDto
    {
        public RuleField Field { get; set; }
        public RuleOperator Operator { get; set; }
        public string? Value { get; set; }
        public string? AttributeName { get; set; }
    }
}
