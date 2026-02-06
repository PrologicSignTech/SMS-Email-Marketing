namespace MarketingPlatform.Core.Entities
{
    public class ContactGroup : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsStatic { get; set; } = true;
        public bool IsDynamic { get; set; } = false;
        public string? RuleCriteria { get; set; }
        public int ContactCount { get; set; } = 0;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<ContactGroupMember> Members { get; set; } = new List<ContactGroupMember>();
        public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();
    }
}
