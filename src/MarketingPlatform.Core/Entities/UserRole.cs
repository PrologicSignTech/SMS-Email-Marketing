namespace MarketingPlatform.Core.Entities
{
    public class UserRole
    {
        public string UserId { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; } // UserId of the person who assigned the role
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
