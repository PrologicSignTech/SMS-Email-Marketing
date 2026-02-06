using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.ContactGroup
{
    public class GroupRuleCriteriaDto
    {
        public RuleLogic Logic { get; set; } = RuleLogic.And;
        public List<GroupRuleDto> Rules { get; set; } = new List<GroupRuleDto>();
    }
}
