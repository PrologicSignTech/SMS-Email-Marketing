using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Keyword;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KeywordsController : ControllerBase
    {
        private readonly IKeywordService _keywordService;
        private readonly ILogger<KeywordsController> _logger;

        public KeywordsController(IKeywordService keywordService, ILogger<KeywordsController> logger)
        {
            _keywordService = keywordService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<KeywordDto>>>> GetKeywords([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var keywords = await _keywordService.GetKeywordsAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<KeywordDto>>.SuccessResponse(keywords));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<KeywordDto>>> GetKeyword(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var keyword = await _keywordService.GetKeywordByIdAsync(userId, id);
            if (keyword == null)
                return NotFound(ApiResponse<KeywordDto>.ErrorResponse("Keyword not found"));

            return Ok(ApiResponse<KeywordDto>.SuccessResponse(keyword));
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<KeywordDto>>>> GetKeywordsByStatus(KeywordStatus status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var keywords = await _keywordService.GetKeywordsByStatusAsync(userId, status);
            return Ok(ApiResponse<List<KeywordDto>>.SuccessResponse(keywords));
        }

        [HttpGet("check-availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAvailability([FromQuery] string keywordText)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(keywordText))
                return BadRequest(ApiResponse<bool>.ErrorResponse("Keyword text is required"));

            var isAvailable = await _keywordService.CheckKeywordAvailabilityAsync(keywordText, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<KeywordDto>>> CreateKeyword([FromBody] CreateKeywordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var keyword = await _keywordService.CreateKeywordAsync(userId, dto);
                return Ok(ApiResponse<KeywordDto>.SuccessResponse(keyword, "Keyword created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create keyword");
                return BadRequest(ApiResponse<KeywordDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating keyword");
                return BadRequest(ApiResponse<KeywordDto>.ErrorResponse("Failed to create keyword", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateKeyword(int id, [FromBody] UpdateKeywordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _keywordService.UpdateKeywordAsync(userId, id, dto);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update keyword. Keyword not found or is globally reserved."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Keyword updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update keyword");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating keyword");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update keyword", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteKeyword(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.DeleteKeywordAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete keyword. Keyword not found or is globally reserved."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Keyword deleted successfully"));
        }

        [HttpGet("{id}/activities")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<KeywordActivityDto>>>> GetKeywordActivities(int id, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var activities = await _keywordService.GetKeywordActivitiesAsync(id, userId, request);
                return Ok(ApiResponse<PaginatedResult<KeywordActivityDto>>.SuccessResponse(activities));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to get keyword activities");
                return BadRequest(ApiResponse<PaginatedResult<KeywordActivityDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keyword activities");
                return BadRequest(ApiResponse<PaginatedResult<KeywordActivityDto>>.ErrorResponse("Failed to get keyword activities", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}/analytics")]
        public async Task<ActionResult<ApiResponse<KeywordAnalyticsDto>>> GetKeywordAnalytics(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var analytics = await _keywordService.GetKeywordAnalyticsAsync(id, userId);
            if (analytics == null)
                return NotFound(ApiResponse<KeywordAnalyticsDto>.ErrorResponse("Keyword not found"));

            return Ok(ApiResponse<KeywordAnalyticsDto>.SuccessResponse(analytics));
        }

        [HttpPost("process-inbound")]
        [AllowAnonymous] // This endpoint should be called by SMS provider webhooks
        // TODO: Add webhook signature validation for production use
        // Example: Validate HMAC signature from Twilio/Plivo headers
        public async Task<ActionResult<ApiResponse<KeywordActivityDto>>> ProcessInboundKeyword([FromBody] InboundSmsDto dto)
        {
            try
            {
                var activity = await _keywordService.ProcessInboundKeywordAsync(dto.PhoneNumber, dto.Message);
                return Ok(ApiResponse<KeywordActivityDto>.SuccessResponse(activity, "Inbound keyword processed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inbound keyword");
                return BadRequest(ApiResponse<KeywordActivityDto>.ErrorResponse("Failed to process inbound keyword", new List<string> { ex.Message }));
            }
        }

        // ======= Keyword Reservation Endpoints (12.4.1) =======
        
        [HttpPost("reservations")]
        public async Task<ActionResult<ApiResponse<KeywordReservationDto>>> CreateReservation([FromBody] CreateKeywordReservationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var reservation = await _keywordService.CreateReservationAsync(userId, dto);
                return Ok(ApiResponse<KeywordReservationDto>.SuccessResponse(reservation, "Keyword reservation created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create keyword reservation");
                return BadRequest(ApiResponse<KeywordReservationDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating keyword reservation");
                return BadRequest(ApiResponse<KeywordReservationDto>.ErrorResponse("Failed to create keyword reservation", new List<string> { ex.Message }));
            }
        }

        [HttpGet("reservations")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<KeywordReservationDto>>>> GetReservations([FromQuery] PagedRequest request)
        {
            var reservations = await _keywordService.GetReservationsAsync(request);
            return Ok(ApiResponse<PaginatedResult<KeywordReservationDto>>.SuccessResponse(reservations));
        }

        [HttpGet("reservations/{id}")]
        public async Task<ActionResult<ApiResponse<KeywordReservationDto>>> GetReservation(int id)
        {
            var reservation = await _keywordService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound(ApiResponse<KeywordReservationDto>.ErrorResponse("Reservation not found"));

            return Ok(ApiResponse<KeywordReservationDto>.SuccessResponse(reservation));
        }

        [HttpPut("reservations/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateReservation(int id, [FromBody] UpdateKeywordReservationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.UpdateReservationAsync(userId, id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Reservation not found or you don't have permission to update it"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Reservation updated successfully"));
        }

        [HttpPost("reservations/{id}/approve")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveReservation(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.ApproveReservationAsync(userId, id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Reservation not found or cannot be approved"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Reservation approved successfully"));
        }

        [HttpPost("reservations/{id}/reject")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectReservation(int id, [FromBody] RejectReservationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.RejectReservationAsync(userId, id, dto.Reason);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Reservation not found or cannot be rejected"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Reservation rejected successfully"));
        }

        // ======= Keyword Assignment Endpoints (12.4.2) =======
        
        [HttpPost("assignments")]
        public async Task<ActionResult<ApiResponse<KeywordAssignmentDto>>> AssignKeyword([FromBody] CreateKeywordAssignmentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var assignment = await _keywordService.AssignKeywordToCampaignAsync(userId, dto);
                return Ok(ApiResponse<KeywordAssignmentDto>.SuccessResponse(assignment, "Keyword assigned to campaign successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to assign keyword");
                return BadRequest(ApiResponse<KeywordAssignmentDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning keyword");
                return BadRequest(ApiResponse<KeywordAssignmentDto>.ErrorResponse("Failed to assign keyword", new List<string> { ex.Message }));
            }
        }

        [HttpGet("assignments")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<KeywordAssignmentDto>>>> GetAssignments([FromQuery] PagedRequest request)
        {
            var assignments = await _keywordService.GetAssignmentsAsync(request);
            return Ok(ApiResponse<PaginatedResult<KeywordAssignmentDto>>.SuccessResponse(assignments));
        }

        [HttpGet("assignments/{id}")]
        public async Task<ActionResult<ApiResponse<KeywordAssignmentDto>>> GetAssignment(int id)
        {
            var assignment = await _keywordService.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound(ApiResponse<KeywordAssignmentDto>.ErrorResponse("Assignment not found"));

            return Ok(ApiResponse<KeywordAssignmentDto>.SuccessResponse(assignment));
        }

        [HttpGet("assignments/campaign/{campaignId}")]
        public async Task<ActionResult<ApiResponse<List<KeywordAssignmentDto>>>> GetAssignmentsByCampaign(int campaignId)
        {
            var assignments = await _keywordService.GetAssignmentsByCampaignAsync(campaignId);
            return Ok(ApiResponse<List<KeywordAssignmentDto>>.SuccessResponse(assignments));
        }

        [HttpPost("assignments/{id}/unassign")]
        public async Task<ActionResult<ApiResponse<bool>>> UnassignKeyword(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.UnassignKeywordAsync(userId, id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Assignment not found or you don't have permission to unassign it"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Keyword unassigned successfully"));
        }

        // ======= Keyword Conflict Resolution Endpoints (12.4.3) =======
        
        [HttpGet("conflicts")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<KeywordConflictDto>>>> GetConflicts([FromQuery] PagedRequest request)
        {
            var conflicts = await _keywordService.GetConflictsAsync(request);
            return Ok(ApiResponse<PaginatedResult<KeywordConflictDto>>.SuccessResponse(conflicts));
        }

        [HttpGet("conflicts/check")]
        public async Task<ActionResult<ApiResponse<KeywordConflictDto>>> CheckForConflict([FromQuery] string keywordText)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(keywordText))
                return BadRequest(ApiResponse<KeywordConflictDto>.ErrorResponse("Keyword text is required"));

            var conflict = await _keywordService.CheckForConflictAsync(keywordText, userId);
            if (conflict == null)
                return Ok(ApiResponse<KeywordConflictDto>.SuccessResponse(null!, "No conflict found"));

            return Ok(ApiResponse<KeywordConflictDto>.SuccessResponse(conflict));
        }

        [HttpPost("conflicts/{id}/resolve")]
        public async Task<ActionResult<ApiResponse<bool>>> ResolveConflict(int id, [FromBody] ResolveKeywordConflictDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _keywordService.ResolveConflictAsync(userId, id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Conflict not found or cannot be resolved"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Conflict resolved successfully"));
        }
    }

    // DTO for inbound SMS webhook
    public class InboundSmsDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // DTO for rejecting reservation
    public class RejectReservationDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
