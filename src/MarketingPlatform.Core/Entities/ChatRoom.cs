using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }
        
        // Guest user information (for non-authenticated users)
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        
        // For authenticated users (nullable)
        public string? CustomerId { get; set; }
        public virtual ApplicationUser? Customer { get; set; }
        
        // Assigned employee/support staff
        public string? AssignedEmployeeId { get; set; }
        public virtual ApplicationUser? AssignedEmployee { get; set; }
        
        public ChatRoomStatus Status { get; set; } = ChatRoomStatus.Open;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
