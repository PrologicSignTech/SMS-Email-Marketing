using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public MessageType MessageType { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public bool IsOwnMessage { get; set; }
    }

    public class SendMessageDto
    {
        public int ChatRoomId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
    }
}
