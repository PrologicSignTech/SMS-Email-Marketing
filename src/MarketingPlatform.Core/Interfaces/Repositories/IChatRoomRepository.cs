using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IChatRoomRepository : IRepository<ChatRoom>
    {
        Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync();
        Task<IEnumerable<ChatRoom>> GetChatRoomsByEmployeeIdAsync(string employeeId);
        Task<ChatRoom?> GetChatRoomByIdWithMessagesAsync(int chatRoomId);
        Task<IEnumerable<ChatRoom>> GetUnassignedChatRoomsAsync();
    }
}
