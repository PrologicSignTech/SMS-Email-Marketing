using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Models
{
    public class GroupRuleCriteria
    {
        public RuleLogic Logic { get; set; } = RuleLogic.And;
        public List<GroupRule> Rules { get; set; } = new List<GroupRule>();
    }
}
