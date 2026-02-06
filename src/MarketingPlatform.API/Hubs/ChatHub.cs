using MarketingPlatform.Application.DTOs.Chat;
using MarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MarketingPlatform.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;
        
        // Track connected users and their connection IDs
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        
        // Track which chat room each connection is in
        private static readonly ConcurrentDictionary<string, int> _connectionChatRooms = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connections[userId] = Context.ConnectionId;
                await _chatService.UpdateUserOnlineStatusAsync(userId, true);
                
                // Notify others that this user is online
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
                
                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connections.TryRemove(userId, out _);
                await _chatService.UpdateUserOnlineStatusAsync(userId, false);
                
                // Notify others that this user is offline
                await Clients.All.SendAsync("UserStatusChanged", userId, false);
                
                _logger.LogInformation("User {UserId} disconnected", userId);
            }
            
            // Clean up chat room tracking
            _connectionChatRooms.TryRemove(Context.ConnectionId, out _);
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            var groupName = GetChatRoomGroupName(chatRoomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _connectionChatRooms[Context.ConnectionId] = chatRoomId;
            
            _logger.LogInformation("Connection {ConnectionId} joined chat room {ChatRoomId}", 
                Context.ConnectionId, chatRoomId);
            
            // Notify others in the room
            await Clients.Group(groupName).SendAsync("UserJoinedRoom", Context.UserIdentifier);
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            var groupName = GetChatRoomGroupName(chatRoomId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _connectionChatRooms.TryRemove(Context.ConnectionId, out _);
            
            _logger.LogInformation("Connection {ConnectionId} left chat room {ChatRoomId}", 
                Context.ConnectionId, chatRoomId);
            
            // Notify others in the room
            await Clients.Group(groupName).SendAsync("UserLeftRoom", Context.UserIdentifier);
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            try
            {
                var userId = Context.UserIdentifier;
                var message = await _chatService.SendMessageAsync(messageDto, userId);
                
                var groupName = GetChatRoomGroupName(messageDto.ChatRoomId);
                
                // Send message to all users in the chat room
                await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
                
                // Also notify the chat list that there's a new message
                await Clients.All.SendAsync("ChatRoomUpdated", messageDto.ChatRoomId);
                
                _logger.LogInformation("Message sent in chat room {ChatRoomId} by user {UserId}", 
                    messageDto.ChatRoomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message in chat room {ChatRoomId}", messageDto.ChatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task NotifyTyping(int chatRoomId, bool isTyping)
        {
            var userId = Context.UserIdentifier;
            var groupName = GetChatRoomGroupName(chatRoomId);
            
            // Notify others in the room (except the sender)
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", userId, isTyping);
        }

        public async Task MarkMessagesAsRead(int chatRoomId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;
            
            await _chatService.MarkMessagesAsReadAsync(chatRoomId, userId);
            
            var groupName = GetChatRoomGroupName(chatRoomId);
            await Clients.Group(groupName).SendAsync("MessagesMarkedAsRead", chatRoomId, userId);
        }

        public async Task AssignChatRoom(int chatRoomId, string employeeId)
        {
            try
            {
                var success = await _chatService.AssignChatRoomAsync(chatRoomId, employeeId);
                
                if (success)
                {
                    // Notify all connected clients about the assignment
                    await Clients.All.SendAsync("ChatRoomAssigned", chatRoomId, employeeId);
                    
                    // Notify the assigned employee specifically
                    if (_connections.TryGetValue(employeeId, out var connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("AssignedToChat", chatRoomId);
                    }
                    
                    _logger.LogInformation("Chat room {ChatRoomId} assigned to employee {EmployeeId}", 
                        chatRoomId, employeeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to assign chat room");
            }
        }

        public async Task CloseChatRoom(int chatRoomId)
        {
            try
            {
                var success = await _chatService.CloseChatRoomAsync(chatRoomId);
                
                if (success)
                {
                    var groupName = GetChatRoomGroupName(chatRoomId);
                    
                    // Notify all users in the chat room
                    await Clients.Group(groupName).SendAsync("ChatRoomClosed", chatRoomId);
                    
                    // Send transcript email if guest chat
                    await _chatService.SendChatTranscriptEmailAsync(chatRoomId);
                    
                    _logger.LogInformation("Chat room {ChatRoomId} closed", chatRoomId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to close chat room");
            }
        }

        private string GetChatRoomGroupName(int chatRoomId)
        {
            return $"ChatRoom_{chatRoomId}";
        }
    }
}
