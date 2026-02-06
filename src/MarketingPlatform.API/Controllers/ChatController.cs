using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Chat;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active chat rooms (for admin/employees)
        /// </summary>
        [HttpGet("rooms")]
        [Authorize]
        [SwaggerOperation(Summary = "Get all active chat rooms")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomDto>>>> GetActiveChatRooms()
        {
            try
            {
                var chatRooms = await _chatService.GetActiveChatRoomsAsync();
                return Ok(ApiResponse<IEnumerable<ChatRoomDto>>.SuccessResponse(chatRooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active chat rooms");
                return StatusCode(500, ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Failed to retrieve chat rooms"));
            }
        }

        /// <summary>
        /// Get unassigned chat rooms (for admin/employees to pick up)
        /// </summary>
        [HttpGet("rooms/unassigned")]
        [Authorize]
        [SwaggerOperation(Summary = "Get unassigned chat rooms")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomDto>>>> GetUnassignedChatRooms()
        {
            try
            {
                var chatRooms = await _chatService.GetUnassignedChatRoomsAsync();
                return Ok(ApiResponse<IEnumerable<ChatRoomDto>>.SuccessResponse(chatRooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unassigned chat rooms");
                return StatusCode(500, ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Failed to retrieve unassigned chat rooms"));
            }
        }

        /// <summary>
        /// Get chat rooms assigned to a specific employee
        /// </summary>
        [HttpGet("rooms/employee/{employeeId}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get chat rooms by employee ID")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomDto>>>> GetChatRoomsByEmployeeId(string employeeId)
        {
            try
            {
                var chatRooms = await _chatService.GetChatRoomsByEmployeeIdAsync(employeeId);
                return Ok(ApiResponse<IEnumerable<ChatRoomDto>>.SuccessResponse(chatRooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat rooms for employee {EmployeeId}", employeeId);
                return StatusCode(500, ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Failed to retrieve employee chat rooms"));
            }
        }

        /// <summary>
        /// Get a specific chat room by ID
        /// </summary>
        [HttpGet("rooms/{id}")]
        [SwaggerOperation(Summary = "Get chat room by ID")]
        public async Task<ActionResult<ApiResponse<ChatRoomDto>>> GetChatRoomById(int id)
        {
            try
            {
                var chatRoom = await _chatService.GetChatRoomByIdAsync(id);
                
                if (chatRoom == null)
                    return NotFound(ApiResponse<ChatRoomDto>.ErrorResponse("Chat room not found"));

                return Ok(ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Failed to retrieve chat room"));
            }
        }

        /// <summary>
        /// Get chat history/messages for a specific chat room
        /// </summary>
        [HttpGet("rooms/{id}/messages")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get chat messages")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatMessageDto>>>> GetChatHistory(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _chatService.GetChatHistoryAsync(id, userId);
                return Ok(ApiResponse<IEnumerable<ChatMessageDto>>.SuccessResponse(messages));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history for room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<IEnumerable<ChatMessageDto>>.ErrorResponse("Failed to retrieve chat history"));
            }
        }

        /// <summary>
        /// Create a new chat room (for customers/guests)
        /// </summary>
        [HttpPost("rooms")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Create a new chat room")]
        public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateChatRoom([FromBody] CreateChatRoomDto? createDto)
        {
            try
            {
                if (createDto == null)
                {
                    _logger.LogWarning("CreateChatRoom called with null DTO");
                    return BadRequest(ApiResponse<ChatRoomDto>.ErrorResponse("Request body is required"));
                }

                if (string.IsNullOrWhiteSpace(createDto.GuestName) && string.IsNullOrWhiteSpace(createDto.CustomerId))
                {
                    _logger.LogWarning("CreateChatRoom called without guest name or customer ID");
                    return BadRequest(ApiResponse<ChatRoomDto>.ErrorResponse("Guest name or customer ID is required"));
                }

                _logger.LogInformation("Creating chat room for guest: {GuestName}, {GuestEmail}",
                    createDto.GuestName, createDto.GuestEmail);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var chatRoom = await _chatService.CreateChatRoomAsync(createDto, userId);

                _logger.LogInformation("Chat room created successfully with ID: {ChatRoomId}", chatRoom?.Id);
                return Ok(ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Chat room created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room. Exception: {Message}, InnerException: {InnerMessage}",
                    ex.Message, ex.InnerException?.Message);
                return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse($"Failed to create chat room: {ex.Message}"));
            }
        }

        /// <summary>
        /// Assign a chat room to an employee (admin only)
        /// </summary>
        [HttpPut("rooms/{id}/assign")]
        [Authorize]
        [SwaggerOperation(Summary = "Assign chat room to employee")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignChatRoom(int id, [FromBody] AssignChatRoomDto assignDto)
        {
            try
            {
                var success = await _chatService.AssignChatRoomAsync(id, assignDto.EmployeeId);
                
                if (!success)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Chat room not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Chat room assigned successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning chat room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to assign chat room"));
            }
        }

        /// <summary>
        /// Close a chat session
        /// </summary>
        [HttpPut("rooms/{id}/close")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Close a chat room")]
        public async Task<ActionResult<ApiResponse<bool>>> CloseChatRoom(int id)
        {
            try
            {
                var success = await _chatService.CloseChatRoomAsync(id);
                
                if (!success)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Chat room not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Chat room closed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing chat room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to close chat room"));
            }
        }

        /// <summary>
        /// Send a message (fallback for non-SignalR clients)
        /// </summary>
        [HttpPost("messages")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Send a chat message")]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage([FromBody] SendMessageDto sendDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var message = await _chatService.SendMessageAsync(sendDto, userId);
                return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(message, "Message sent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, ApiResponse<ChatMessageDto>.ErrorResponse("Failed to send message"));
            }
        }

        /// <summary>
        /// Send chat transcript to user's email
        /// </summary>
        [HttpPost("rooms/{id}/send-transcript")]
        [Authorize]
        [SwaggerOperation(Summary = "Send chat transcript via email")]
        public async Task<ActionResult<ApiResponse<bool>>> SendChatTranscript(int id)
        {
            try
            {
                var success = await _chatService.SendChatTranscriptEmailAsync(id);
                
                if (!success)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Chat room not found or email not available"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Chat transcript sent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat transcript for room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to send chat transcript"));
            }
        }

        /// <summary>
        /// Get unread message count for an employee
        /// </summary>
        [HttpGet("unread-count/{employeeId}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get unread message count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadMessageCount(string employeeId)
        {
            try
            {
                var count = await _chatService.GetUnreadMessageCountAsync(employeeId);
                return Ok(ApiResponse<int>.SuccessResponse(count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count for employee {EmployeeId}", employeeId);
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Failed to get unread message count"));
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPost("rooms/{id}/mark-read")]
        [Authorize]
        [SwaggerOperation(Summary = "Mark messages as read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkMessagesAsRead(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

                await _chatService.MarkMessagesAsReadAsync(id, userId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Messages marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for room {ChatRoomId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to mark messages as read"));
            }
        }
    }
}
