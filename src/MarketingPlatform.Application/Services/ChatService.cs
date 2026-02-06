using MarketingPlatform.Application.DTOs.Chat;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MarketingPlatform.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailProvider _emailProvider;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRoomRepository chatRoomRepository,
            IChatMessageRepository chatMessageRepository,
            IRepository<ApplicationUser> userRepository,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IEmailProvider emailProvider,
            ILogger<ChatService> logger)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatMessageRepository = chatMessageRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailProvider = emailProvider;
            _logger = logger;
        }

        public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto, string? userId = null)
        {
            try
            {
                _logger.LogInformation("Creating chat room entity for guest: {GuestName}", createDto?.GuestName);

                var chatRoom = new ChatRoom
                {
                    GuestName = createDto?.GuestName,
                    GuestEmail = createDto?.GuestEmail,
                    CustomerId = userId ?? createDto?.CustomerId,
                    Status = ChatRoomStatus.Open,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Adding chat room to repository...");
                await _chatRoomRepository.AddAsync(chatRoom);

                _logger.LogInformation("Saving changes to database...");
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created chat room {ChatRoomId} for {GuestName}", chatRoom.Id, createDto?.GuestName);

                return await MapToChatRoomDto(chatRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room. Error: {Message}, Inner: {InnerMessage}",
                    ex.Message, ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<ChatRoomDto?> GetChatRoomByIdAsync(int chatRoomId)
        {
            var chatRoom = await _chatRoomRepository.GetChatRoomByIdWithMessagesAsync(chatRoomId);
            if (chatRoom == null)
                return null;

            return await MapToChatRoomDto(chatRoom);
        }

        public async Task<IEnumerable<ChatRoomDto>> GetActiveChatRoomsAsync()
        {
            var chatRooms = await _chatRoomRepository.GetActiveChatRoomsAsync();
            var dtos = new List<ChatRoomDto>();

            foreach (var room in chatRooms)
            {
                dtos.Add(await MapToChatRoomDto(room));
            }

            return dtos;
        }

        public async Task<IEnumerable<ChatRoomDto>> GetChatRoomsByEmployeeIdAsync(string employeeId)
        {
            var chatRooms = await _chatRoomRepository.GetChatRoomsByEmployeeIdAsync(employeeId);
            var dtos = new List<ChatRoomDto>();

            foreach (var room in chatRooms)
            {
                dtos.Add(await MapToChatRoomDto(room));
            }

            return dtos;
        }

        public async Task<IEnumerable<ChatRoomDto>> GetUnassignedChatRoomsAsync()
        {
            var chatRooms = await _chatRoomRepository.GetUnassignedChatRoomsAsync();
            var dtos = new List<ChatRoomDto>();

            foreach (var room in chatRooms)
            {
                dtos.Add(await MapToChatRoomDto(room));
            }

            return dtos;
        }

        public async Task<bool> AssignChatRoomAsync(int chatRoomId, string employeeId)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return false;

            chatRoom.AssignedEmployeeId = employeeId;
            chatRoom.Status = ChatRoomStatus.InProgress;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            _chatRoomRepository.Update(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Assigned chat room {ChatRoomId} to employee {EmployeeId}", chatRoomId, employeeId);

            return true;
        }

        public async Task<bool> CloseChatRoomAsync(int chatRoomId)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return false;

            chatRoom.Status = ChatRoomStatus.Closed;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            _chatRoomRepository.Update(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Closed chat room {ChatRoomId}", chatRoomId);

            return true;
        }

        public async Task<bool> UpdateChatRoomStatusAsync(int chatRoomId, ChatRoomStatus status)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return false;

            chatRoom.Status = status;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            _chatRoomRepository.Update(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto sendDto, string? senderId)
        {
            var message = new ChatMessage
            {
                ChatRoomId = sendDto.ChatRoomId,
                SenderId = senderId,
                MessageText = sendDto.MessageText,
                MessageType = sendDto.MessageType,
                AttachmentUrl = sendDto.AttachmentUrl,
                AttachmentFileName = sendDto.AttachmentFileName,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            await _chatMessageRepository.AddAsync(message);

            // Update chat room's UpdatedAt
            var chatRoom = await _chatRoomRepository.GetByIdAsync(sendDto.ChatRoomId);
            if (chatRoom != null)
            {
                chatRoom.UpdatedAt = DateTime.UtcNow;
                _chatRoomRepository.Update(chatRoom);
            }

            await _unitOfWork.SaveChangesAsync();

            return await MapToChatMessageDto(message);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int chatRoomId, string? currentUserId = null)
        {
            var messages = await _chatMessageRepository.GetMessagesByChatRoomIdAsync(chatRoomId);
            var dtos = new List<ChatMessageDto>();

            foreach (var message in messages)
            {
                var dto = await MapToChatMessageDto(message);
                dto.IsOwnMessage = message.SenderId == currentUserId;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<int> GetUnreadMessageCountAsync(string employeeId)
        {
            return await _chatMessageRepository.GetUnreadMessageCountByEmployeeIdAsync(employeeId);
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, string userId)
        {
            await _chatMessageRepository.MarkMessagesAsReadAsync(chatRoomId, userId);
        }

        public async Task<string> GenerateChatTranscriptAsync(int chatRoomId)
        {
            var chatRoom = await _chatRoomRepository.GetChatRoomByIdWithMessagesAsync(chatRoomId);
            if (chatRoom == null)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("=== Chat Transcript ===");
            sb.AppendLine($"Chat Room ID: {chatRoom.Id}");
            sb.AppendLine($"Customer: {chatRoom.GuestName ?? chatRoom.Customer?.UserName ?? "Unknown"}");
            sb.AppendLine($"Email: {chatRoom.GuestEmail ?? chatRoom.Customer?.Email ?? "N/A"}");
            sb.AppendLine($"Started: {chatRoom.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Closed: {chatRoom.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("========================\n");

            foreach (var message in chatRoom.Messages.OrderBy(m => m.SentAt))
            {
                var senderName = message.Sender?.UserName ?? chatRoom.GuestName ?? "Guest";
                sb.AppendLine($"[{message.SentAt:yyyy-MM-dd HH:mm:ss}] {senderName}:");
                sb.AppendLine($"  {message.MessageText}");
                if (!string.IsNullOrEmpty(message.AttachmentUrl))
                {
                    sb.AppendLine($"  Attachment: {message.AttachmentFileName}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<bool> SendChatTranscriptEmailAsync(int chatRoomId)
        {
            try
            {
                var chatRoom = await _chatRoomRepository.GetChatRoomByIdWithMessagesAsync(chatRoomId);
                if (chatRoom == null || string.IsNullOrEmpty(chatRoom.GuestEmail))
                    return false;

                var transcript = await GenerateChatTranscriptAsync(chatRoomId);

                var emailSubject = "Your Chat Transcript - Marketing Platform Support";
                var emailBody = $@"
                    <html>
                    <body>
                        <h2>Thank you for contacting our support!</h2>
                        <p>Here is a transcript of your chat session:</p>
                        <pre style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
{transcript}
                        </pre>
                        <p>If you have any further questions, please don't hesitate to contact us.</p>
                        <p>Best regards,<br/>Marketing Platform Support Team</p>
                    </body>
                    </html>
                ";

                await _emailProvider.SendEmailAsync(chatRoom.GuestEmail, emailSubject, emailBody);

                _logger.LogInformation("Sent chat transcript email to {Email} for chat room {ChatRoomId}", 
                    chatRoom.GuestEmail, chatRoomId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send chat transcript email for chat room {ChatRoomId}", chatRoomId);
                return false;
            }
        }

        public async Task UpdateUserOnlineStatusAsync(string userId, bool isOnline)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return;

            user.IsOnline = isOnline;
            user.LastSeenAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
        }

        // Helper methods
        private async Task<ChatRoomDto> MapToChatRoomDto(ChatRoom chatRoom)
        {
            var lastMessage = chatRoom.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unreadCount = chatRoom.Messages?.Count(m => !m.IsRead && m.SenderId != chatRoom.AssignedEmployeeId) ?? 0;

            return new ChatRoomDto
            {
                Id = chatRoom.Id,
                GuestName = chatRoom.GuestName,
                GuestEmail = chatRoom.GuestEmail,
                CustomerId = chatRoom.CustomerId,
                CustomerName = chatRoom.Customer != null 
                    ? $"{chatRoom.Customer.FirstName} {chatRoom.Customer.LastName}".Trim() 
                    : chatRoom.GuestName,
                AssignedEmployeeId = chatRoom.AssignedEmployeeId,
                AssignedEmployeeName = chatRoom.AssignedEmployee != null 
                    ? $"{chatRoom.AssignedEmployee.FirstName} {chatRoom.AssignedEmployee.LastName}".Trim() 
                    : null,
                Status = chatRoom.Status,
                CreatedAt = chatRoom.CreatedAt,
                UpdatedAt = chatRoom.UpdatedAt,
                UnreadCount = unreadCount,
                LastMessage = lastMessage?.MessageText,
                LastMessageTime = lastMessage?.SentAt
            };
        }

        private async Task<ChatMessageDto> MapToChatMessageDto(ChatMessage message)
        {
            var senderName = message.Sender != null 
                ? $"{message.Sender.FirstName} {message.Sender.LastName}".Trim() 
                : "Guest";

            return new ChatMessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderId = message.SenderId,
                SenderName = senderName,
                MessageText = message.MessageText,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                MessageType = message.MessageType,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentFileName = message.AttachmentFileName,
                IsOwnMessage = false // Will be set by the calling method
            };
        }
    }
}
