namespace MarketingPlatform.Application.DTOs.ContactGroup
{
    public class ContactGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDynamic { get; set; }
        public GroupRuleCriteriaDto? RuleCriteria { get; set; }
        public int ContactCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
