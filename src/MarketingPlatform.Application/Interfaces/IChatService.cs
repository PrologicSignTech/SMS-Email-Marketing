using MarketingPlatform.Application.DTOs.Chat;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IChatService
    {
        // Chat Room operations
        Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto, string? userId = null);
        Task<ChatRoomDto?> GetChatRoomByIdAsync(int chatRoomId);
        Task<IEnumerable<ChatRoomDto>> GetActiveChatRoomsAsync();
        Task<IEnumerable<ChatRoomDto>> GetChatRoomsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<ChatRoomDto>> GetUnassignedChatRoomsAsync();
        Task<bool> AssignChatRoomAsync(int chatRoomId, string employeeId);
        Task<bool> CloseChatRoomAsync(int chatRoomId);
        Task<bool> UpdateChatRoomStatusAsync(int chatRoomId, ChatRoomStatus status);
        
        // Chat Message operations
        Task<ChatMessageDto> SendMessageAsync(SendMessageDto sendDto, string? senderId);
        Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int chatRoomId, string? currentUserId = null);
        Task<int> GetUnreadMessageCountAsync(string employeeId);
        Task MarkMessagesAsReadAsync(int chatRoomId, string userId);
        
        // Chat Transcript
        Task<string> GenerateChatTranscriptAsync(int chatRoomId);
        Task<bool> SendChatTranscriptEmailAsync(int chatRoomId);
        
        // User Status
        Task UpdateUserOnlineStatusAsync(string userId, bool isOnline);
    }
}
