namespace MarketingPlatform.Core.Entities
{
    public class ContactTagAssignment : BaseEntity
    {
        public int ContactId { get; set; }
        public int ContactTagId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
        public virtual ContactTag ContactTag { get; set; } = null!;
    }
}
