using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionList;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SuppressionListsController : ControllerBase
    {
        private readonly ISuppressionListService _suppressionService;
        private readonly ILogger<SuppressionListsController> _logger;

        public SuppressionListsController(ISuppressionListService suppressionService, ILogger<SuppressionListsController> logger)
        {
            _suppressionService = suppressionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<SuppressionListDto>>>> GetSuppressionLists([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _suppressionService.GetAllAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<SuppressionListDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SuppressionListDto>>> GetSuppressionList(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _suppressionService.GetByIdAsync(userId, id);
            if (result == null)
                return NotFound(ApiResponse<SuppressionListDto>.ErrorResponse("Suppression entry not found"));

            return Ok(ApiResponse<SuppressionListDto>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SuppressionListDto>>> CreateSuppressionEntry([FromBody] CreateSuppressionListDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _suppressionService.CreateAsync(userId, dto);
                return Ok(ApiResponse<SuppressionListDto>.SuccessResponse(result, "Suppression entry created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating suppression entry");
                return BadRequest(ApiResponse<SuppressionListDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<ApiResponse<int>>> BulkCreateSuppressionEntries([FromBody] BulkSuppressionDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _suppressionService.BulkCreateAsync(userId, dto);
                return Ok(ApiResponse<int>.SuccessResponse(result, $"{result} suppression entries created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk suppression entries");
                return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSuppressionEntry(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _suppressionService.DeleteAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Suppression entry not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Suppression entry deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting suppression entry");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("check/{phoneOrEmail}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckIfSuppressed(string phoneOrEmail)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _suppressionService.IsSuppressedAsync(userId, phoneOrEmail);
            return Ok(ApiResponse<bool>.SuccessResponse(result));
        }
    }
}
