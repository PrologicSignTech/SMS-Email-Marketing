namespace MarketingPlatform.Application.DTOs.ContactGroup
{
    public class CreateContactGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsStatic { get; set; } = true;
        public bool IsDynamic { get; set; } = false;
        public GroupRuleCriteriaDto? RuleCriteria { get; set; }
    }
}
