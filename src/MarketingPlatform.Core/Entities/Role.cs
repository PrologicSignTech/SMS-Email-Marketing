using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Permissions { get; set; } // Store permissions as flags
        public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        // Helper method to check if role has a specific permission
        public bool HasPermission(Permission permission)
        {
            return (Permissions & (long)permission) != 0;
        }
        
        // Helper method to add permission
        public void AddPermission(Permission permission)
        {
            Permissions |= (long)permission;
        }
        
        // Helper method to remove permission
        public void RemovePermission(Permission permission)
        {
            Permissions &= ~(long)permission;
        }
    }
}
