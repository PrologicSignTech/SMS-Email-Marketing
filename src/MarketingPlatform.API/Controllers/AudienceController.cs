using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Audience;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AudienceController : ControllerBase
    {
        private readonly IAudienceSegmentationService _audienceService;
        private readonly ILogger<AudienceController> _logger;

        public AudienceController(IAudienceSegmentationService audienceService, ILogger<AudienceController> logger)
        {
            _audienceService = audienceService;
            _logger = logger;
        }

        [HttpPost("evaluate")]
        public async Task<ActionResult<ApiResponse<AudienceSegmentDto>>> EvaluateSegment([FromBody] SegmentCriteriaDto criteria)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _audienceService.EvaluateSegmentAsync(userId, criteria);
                return Ok(ApiResponse<AudienceSegmentDto>.SuccessResponse(result, "Segment evaluated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating segment");
                return BadRequest(ApiResponse<AudienceSegmentDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("calculate-size")]
        public async Task<ActionResult<ApiResponse<int>>> CalculateAudienceSize([FromBody] SegmentCriteriaDto criteria)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _audienceService.CalculateAudienceSizeAsync(userId, criteria);
                return Ok(ApiResponse<int>.SuccessResponse(result, "Audience size calculated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating audience size");
                return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("groups/{groupId}/refresh")]
        public async Task<ActionResult<ApiResponse<bool>>> RefreshDynamicGroup(int groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _audienceService.UpdateDynamicGroupMembersAsync(userId, groupId);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to refresh dynamic group. Group not found or not configured as dynamic."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Dynamic group refreshed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dynamic group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
