using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId);
        Task<int> GetUnreadMessageCountByEmployeeIdAsync(string employeeId);
        Task MarkMessagesAsReadAsync(int chatRoomId, string userId);
    }
}
