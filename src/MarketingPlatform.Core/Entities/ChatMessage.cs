using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        public int ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; } = null!;
        
        // Sender can be authenticated user or guest (use GuestName from ChatRoom)
        public string? SenderId { get; set; }
        public virtual ApplicationUser? Sender { get; set; }
        
        public string MessageText { get; set; } = string.Empty;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public MessageType MessageType { get; set; } = MessageType.Text;
        
        // For file/image attachments
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
    }
}
