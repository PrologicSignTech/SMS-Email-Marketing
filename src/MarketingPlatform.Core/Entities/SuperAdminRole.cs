namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Tracks super admin role assignments with full audit trail
    /// </summary>
    public class SuperAdminRole : BaseEntity
    {
        /// <summary>
        /// User ID who has been assigned super admin role
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// When the super admin role was assigned
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who assigned this super admin role
        /// </summary>
        public string? AssignedBy { get; set; }

        /// <summary>
        /// Reason for assigning super admin privileges
        /// </summary>
        public string AssignmentReason { get; set; } = string.Empty;

        /// <summary>
        /// When the super admin role was revoked (null if still active)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// User ID who revoked this super admin role
        /// </summary>
        public string? RevokedBy { get; set; }

        /// <summary>
        /// Reason for revoking super admin privileges
        /// </summary>
        public string? RevocationReason { get; set; }

        /// <summary>
        /// Whether this super admin assignment is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? AssignedByUser { get; set; }
        public virtual ApplicationUser? RevokedByUser { get; set; }
    }
}
