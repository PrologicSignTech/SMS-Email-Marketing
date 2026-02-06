using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadMessageCountByEmployeeIdAsync(string employeeId)
        {
            var assignedChatRoomIds = await _context.ChatRooms
                .Where(c => c.AssignedEmployeeId == employeeId)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.ChatMessages
                .Where(m => assignedChatRoomIds.Contains(m.ChatRoomId) 
                    && !m.IsRead 
                    && m.SenderId != employeeId)
                .CountAsync();
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, string userId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsRead && m.SenderId != userId)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
