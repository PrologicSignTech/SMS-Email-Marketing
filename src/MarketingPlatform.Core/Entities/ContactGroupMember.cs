namespace MarketingPlatform.Core.Entities
{
    public class ContactGroupMember : BaseEntity
    {
        public int ContactId { get; set; }
        public int ContactGroupId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
        public virtual ContactGroup ContactGroup { get; set; } = null!;
    }
}
