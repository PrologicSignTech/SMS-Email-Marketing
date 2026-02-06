using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Chat
{
    public class ChatRoomDto
    {
        public int Id { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? AssignedEmployeeId { get; set; }
        public string? AssignedEmployeeName { get; set; }
        public ChatRoomStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }

    public class CreateChatRoomDto
    {
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? CustomerId { get; set; }
    }

    public class AssignChatRoomDto
    {
        public string EmployeeId { get; set; } = string.Empty;
    }
}
