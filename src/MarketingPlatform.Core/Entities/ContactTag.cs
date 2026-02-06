namespace MarketingPlatform.Core.Entities
{
    public class ContactTag : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<ContactTagAssignment> TagAssignments { get; set; } = new List<ContactTagAssignment>();
    }
}
