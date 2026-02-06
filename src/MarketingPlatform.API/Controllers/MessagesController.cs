using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<MessageDto>>>> GetMessages([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var messages = await _messageService.GetMessagesAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<MessageDto>>.SuccessResponse(messages));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MessageDto>>> GetMessage(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = await _messageService.GetMessageByIdAsync(userId, id);
            if (message == null)
                return NotFound(ApiResponse<MessageDto>.ErrorResponse("Message not found"));

            return Ok(ApiResponse<MessageDto>.SuccessResponse(message));
        }

        [HttpGet("campaign/{campaignId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<MessageDto>>>> GetCampaignMessages(
            int campaignId, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var messages = await _messageService.GetMessagesByCampaignAsync(userId, campaignId, request);
                return Ok(ApiResponse<PaginatedResult<MessageDto>>.SuccessResponse(messages));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<MessageDto>>>> GetMessagesByStatus(int status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!Enum.IsDefined(typeof(MessageStatus), status))
                return BadRequest(ApiResponse<List<MessageDto>>.ErrorResponse("Invalid status value"));

            var messages = await _messageService.GetMessagesByStatusAsync(userId, (MessageStatus)status);
            return Ok(ApiResponse<List<MessageDto>>.SuccessResponse(messages));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MessageDto>>> CreateMessage([FromBody] CreateMessageDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var message = await _messageService.CreateMessageAsync(userId, dto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, 
                    ApiResponse<MessageDto>.SuccessResponse(message, "Message created and queued"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<ApiResponse<List<MessageDto>>>> CreateBulkMessages([FromBody] BulkMessageRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var messages = await _messageService.CreateBulkMessagesAsync(userId, dto);
                return Ok(ApiResponse<List<MessageDto>>.SuccessResponse(messages, 
                    $"{messages.Count} messages created and queued"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<List<MessageDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/send")]
        public async Task<ActionResult<ApiResponse<bool>>> SendMessageNow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _messageService.SendMessageNowAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Message not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Message sent immediately"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/retry")]
        public async Task<ActionResult<ApiResponse<bool>>> RetryFailedMessage(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _messageService.RetryFailedMessageAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Message not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Message queued for retry"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelScheduledMessage(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _messageService.CancelScheduledMessageAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Message not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Message cancelled"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("campaign/{campaignId}/retry")]
        public async Task<ActionResult<ApiResponse<int>>> RetryFailedMessagesForCampaign(int campaignId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var count = await _messageService.RetryFailedMessagesForCampaignAsync(userId, campaignId);
                return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} messages queued for retry"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("campaign/{campaignId}/report")]
        public async Task<ActionResult<ApiResponse<MessageDeliveryReportDto>>> GetDeliveryReport(int campaignId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var report = await _messageService.GetDeliveryReportAsync(userId, campaignId);
                return Ok(ApiResponse<MessageDeliveryReportDto>.SuccessResponse(report));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Preview message content with variable substitution and device-specific rendering
        /// </summary>
        [HttpPost("preview")]
        public async Task<ActionResult<ApiResponse<MessagePreviewDto>>> PreviewMessage([FromBody] MessagePreviewRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var preview = await _messageService.PreviewMessageAsync(userId, request);
                return Ok(ApiResponse<MessagePreviewDto>.SuccessResponse(preview));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<MessagePreviewDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing message");
                return StatusCode(500, ApiResponse<MessagePreviewDto>.ErrorResponse("Failed to preview message"));
            }
        }

        /// <summary>
        /// Send test message to specified recipients
        /// </summary>
        [HttpPost("test-send")]
        public async Task<ActionResult<ApiResponse<TestSendResultDto>>> SendTestMessage([FromBody] TestSendRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _messageService.SendTestMessageAsync(userId, request);
                
                if (result.IsSuccess)
                {
                    return Ok(ApiResponse<TestSendResultDto>.SuccessResponse(result, 
                        $"Test message sent successfully to {result.SuccessCount} recipient(s)"));
                }
                else
                {
                    return Ok(ApiResponse<TestSendResultDto>.SuccessResponse(result,
                        $"Test send completed with {result.SuccessCount} success(es) and {result.FailureCount} failure(s)"));
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<TestSendResultDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test message");
                return StatusCode(500, ApiResponse<TestSendResultDto>.ErrorResponse("Failed to send test message"));
            }
        }
    }
}
