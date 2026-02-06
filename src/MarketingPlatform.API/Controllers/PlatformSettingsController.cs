using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformSettingsController : ControllerBase
    {
        private readonly IPlatformSettingService _settingService;
        private readonly ILogger<PlatformSettingsController> _logger;

        public PlatformSettingsController(
            IPlatformSettingService settingService,
            ILogger<PlatformSettingsController> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all platform settings with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<PlatformSettingDto>>>> GetSettings([FromQuery] PagedRequest request)
        {
            try
            {
                var settings = await _settingService.GetSettingsAsync(request);
                return Ok(ApiResponse<PaginatedResult<PlatformSettingDto>>.SuccessResponse(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform settings");
                return StatusCode(500, ApiResponse<PaginatedResult<PlatformSettingDto>>.ErrorResponse("Error retrieving settings"));
            }
        }

        /// <summary>
        /// Get platform setting by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PlatformSettingDto>>> GetSettingById(int id)
        {
            try
            {
                var setting = await _settingService.GetSettingByIdAsync(id);
                if (setting == null)
                    return NotFound(ApiResponse<PlatformSettingDto>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<PlatformSettingDto>.SuccessResponse(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform setting {Id}", id);
                return StatusCode(500, ApiResponse<PlatformSettingDto>.ErrorResponse("Error retrieving setting"));
            }
        }

        /// <summary>
        /// Get platform setting by key
        /// </summary>
        [HttpGet("key/{key}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PlatformSettingDto>>> GetSettingByKey(string key)
        {
            try
            {
                var setting = await _settingService.GetSettingByKeyAsync(key);
                if (setting == null)
                    return NotFound(ApiResponse<PlatformSettingDto>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<PlatformSettingDto>.SuccessResponse(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform setting with key {Key}", key);
                return StatusCode(500, ApiResponse<PlatformSettingDto>.ErrorResponse("Error retrieving setting"));
            }
        }

        /// <summary>
        /// Get platform settings by category
        /// </summary>
        [HttpGet("category/{category}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<PlatformSettingDto>>>> GetSettingsByCategory(string category)
        {
            try
            {
                var settings = await _settingService.GetSettingsByCategoryAsync(category);
                return Ok(ApiResponse<List<PlatformSettingDto>>.SuccessResponse(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform settings for category {Category}", category);
                return StatusCode(500, ApiResponse<List<PlatformSettingDto>>.ErrorResponse("Error retrieving settings"));
            }
        }

        /// <summary>
        /// Create a new platform setting
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PlatformSettingDto>>> CreateSetting([FromBody] CreatePlatformSettingDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var setting = await _settingService.CreateSettingAsync(dto, userId);
                return CreatedAtAction(nameof(GetSettingById), new { id = setting.Id },
                    ApiResponse<PlatformSettingDto>.SuccessResponse(setting, "Setting created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PlatformSettingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating platform setting");
                return StatusCode(500, ApiResponse<PlatformSettingDto>.ErrorResponse("Error creating setting"));
            }
        }

        /// <summary>
        /// Update an existing platform setting
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSetting(int id, [FromBody] UpdatePlatformSettingDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _settingService.UpdateSettingAsync(id, dto, userId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Setting updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating platform setting {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating setting"));
            }
        }

        /// <summary>
        /// Delete a platform setting
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSetting(int id)
        {
            try
            {
                var result = await _settingService.DeleteSettingAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Setting deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting platform setting {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting setting"));
            }
        }
    }
}
