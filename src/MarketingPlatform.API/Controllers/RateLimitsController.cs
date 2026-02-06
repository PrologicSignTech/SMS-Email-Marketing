using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.RateLimit;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimitsController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RateLimitsController> _logger;

        public RateLimitsController(IRateLimitService rateLimitService, ILogger<RateLimitsController> logger)
        {
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        /// <summary>
        /// Get rate limit status for current user
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<RateLimitStatusDto>>> GetRateLimitStatus([FromQuery] string endpoint)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var tenantId = User.FindFirst("TenantId")?.Value;
            var status = await _rateLimitService.CheckApiRateLimitAsync(userId, tenantId, endpoint, "GET");

            return Ok(ApiResponse<RateLimitStatusDto>.SuccessResponse(status));
        }

        /// <summary>
        /// Get all API rate limits (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Permission:ManageSettings")]
        public async Task<ActionResult<ApiResponse<List<ApiRateLimitDto>>>> GetApiRateLimits(
            [FromQuery] string? userId = null,
            [FromQuery] string? tenantId = null)
        {
            var rateLimits = await _rateLimitService.GetApiRateLimitsAsync(userId, tenantId);
            return Ok(ApiResponse<List<ApiRateLimitDto>>.SuccessResponse(rateLimits));
        }

        /// <summary>
        /// Create a new API rate limit rule (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Permission:ManageSettings")]
        public async Task<ActionResult<ApiResponse<ApiRateLimitDto>>> CreateApiRateLimit([FromBody] CreateApiRateLimitDto dto)
        {
            try
            {
                var rateLimit = await _rateLimitService.CreateApiRateLimitAsync(dto);
                return CreatedAtAction(nameof(GetApiRateLimits), new { }, 
                    ApiResponse<ApiRateLimitDto>.SuccessResponse(rateLimit, "Rate limit created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API rate limit");
                return BadRequest(ApiResponse<ApiRateLimitDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update an existing API rate limit rule (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "Permission:ManageSettings")]
        public async Task<ActionResult<ApiResponse<ApiRateLimitDto>>> UpdateApiRateLimit(int id, [FromBody] UpdateApiRateLimitDto dto)
        {
            try
            {
                var rateLimit = await _rateLimitService.UpdateApiRateLimitAsync(id, dto);
                return Ok(ApiResponse<ApiRateLimitDto>.SuccessResponse(rateLimit, "Rate limit updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ApiRateLimitDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API rate limit {Id}", id);
                return BadRequest(ApiResponse<ApiRateLimitDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete an API rate limit rule (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Permission:ManageSettings")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteApiRateLimit(int id)
        {
            try
            {
                await _rateLimitService.DeleteApiRateLimitAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Rate limit deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API rate limit {Id}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get rate limit violation logs (admin only)
        /// </summary>
        [HttpGet("logs")]
        [Authorize(Policy = "Permission:ManageSettings")]
        public async Task<ActionResult<ApiResponse<List<RateLimitLogDto>>>> GetRateLimitLogs(
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var logs = await _rateLimitService.GetRateLimitLogsAsync(userId, startDate, endDate, pageSize);
                return Ok(ApiResponse<List<RateLimitLogDto>>.SuccessResponse(logs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rate limit logs");
                return BadRequest(ApiResponse<List<RateLimitLogDto>>.ErrorResponse(ex.Message));
            }
        }
    }
}
