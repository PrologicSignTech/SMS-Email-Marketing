using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class ChatRoomRepository : Repository<ChatRoom>, IChatRoomRepository
    {
        public ChatRoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync()
        {
            return await _context.ChatRooms
                .Include(c => c.Customer)
                .Include(c => c.AssignedEmployee)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.Status != ChatRoomStatus.Closed)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsByEmployeeIdAsync(string employeeId)
        {
            return await _context.ChatRooms
                .Include(c => c.Customer)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.AssignedEmployeeId == employeeId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ChatRoom?> GetChatRoomByIdWithMessagesAsync(int chatRoomId)
        {
            return await _context.ChatRooms
                .Include(c => c.Customer)
                .Include(c => c.AssignedEmployee)
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == chatRoomId);
        }

        public async Task<IEnumerable<ChatRoom>> GetUnassignedChatRoomsAsync()
        {
            return await _context.ChatRooms
                .Include(c => c.Customer)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.AssignedEmployeeId == null && c.Status == ChatRoomStatus.Open)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
