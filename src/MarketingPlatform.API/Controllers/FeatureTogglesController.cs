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
    public class FeatureTogglesController : ControllerBase
    {
        private readonly IFeatureToggleService _featureToggleService;
        private readonly ILogger<FeatureTogglesController> _logger;

        public FeatureTogglesController(
            IFeatureToggleService featureToggleService,
            ILogger<FeatureTogglesController> logger)
        {
            _featureToggleService = featureToggleService;
            _logger = logger;
        }

        /// <summary>
        /// Get all feature toggles with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<FeatureToggleDto>>>> GetFeatureToggles([FromQuery] PagedRequest request)
        {
            try
            {
                var features = await _featureToggleService.GetFeatureTogglesAsync(request);
                return Ok(ApiResponse<PaginatedResult<FeatureToggleDto>>.SuccessResponse(features));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggles");
                return StatusCode(500, ApiResponse<PaginatedResult<FeatureToggleDto>>.ErrorResponse("Error retrieving feature toggles"));
            }
        }

        /// <summary>
        /// Get feature toggle by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<FeatureToggleDto>>> GetFeatureToggleById(int id)
        {
            try
            {
                var feature = await _featureToggleService.GetFeatureToggleByIdAsync(id);
                if (feature == null)
                    return NotFound(ApiResponse<FeatureToggleDto>.ErrorResponse("Feature toggle not found"));

                return Ok(ApiResponse<FeatureToggleDto>.SuccessResponse(feature));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggle {Id}", id);
                return StatusCode(500, ApiResponse<FeatureToggleDto>.ErrorResponse("Error retrieving feature toggle"));
            }
        }

        /// <summary>
        /// Get feature toggle by name
        /// </summary>
        [HttpGet("name/{name}")]
        public async Task<ActionResult<ApiResponse<FeatureToggleDto>>> GetFeatureToggleByName(string name)
        {
            try
            {
                var feature = await _featureToggleService.GetFeatureToggleByNameAsync(name);
                if (feature == null)
                    return NotFound(ApiResponse<FeatureToggleDto>.ErrorResponse("Feature toggle not found"));

                return Ok(ApiResponse<FeatureToggleDto>.SuccessResponse(feature));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggle {Name}", name);
                return StatusCode(500, ApiResponse<FeatureToggleDto>.ErrorResponse("Error retrieving feature toggle"));
            }
        }

        /// <summary>
        /// Get feature toggles by category
        /// </summary>
        [HttpGet("category/{category}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<FeatureToggleDto>>>> GetFeatureTogglesByCategory(string category)
        {
            try
            {
                var features = await _featureToggleService.GetFeatureTogglesByCategoryAsync(category);
                return Ok(ApiResponse<List<FeatureToggleDto>>.SuccessResponse(features));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature toggles for category {Category}", category);
                return StatusCode(500, ApiResponse<List<FeatureToggleDto>>.ErrorResponse("Error retrieving feature toggles"));
            }
        }

        /// <summary>
        /// Check if a feature is enabled globally
        /// </summary>
        [HttpGet("{name}/enabled")]
        public async Task<ActionResult<ApiResponse<bool>>> IsFeatureEnabled(string name)
        {
            try
            {
                var isEnabled = await _featureToggleService.IsFeatureEnabledAsync(name);
                return Ok(ApiResponse<bool>.SuccessResponse(isEnabled));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if feature {Name} is enabled", name);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error checking feature status"));
            }
        }

        /// <summary>
        /// Check if a feature is enabled for current user
        /// </summary>
        [HttpGet("{name}/enabled/me")]
        public async Task<ActionResult<ApiResponse<bool>>> IsFeatureEnabledForCurrentUser(string name)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var isEnabled = await _featureToggleService.IsFeatureEnabledForUserAsync(name, userId);
                return Ok(ApiResponse<bool>.SuccessResponse(isEnabled));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if feature {Name} is enabled for user", name);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error checking feature status"));
            }
        }

        /// <summary>
        /// Check if a feature is enabled for a specific role
        /// </summary>
        [HttpGet("{name}/enabled/role/{roleName}")]
        public async Task<ActionResult<ApiResponse<bool>>> IsFeatureEnabledForRole(string name, string roleName)
        {
            try
            {
                var isEnabled = await _featureToggleService.IsFeatureEnabledForRoleAsync(name, roleName);
                return Ok(ApiResponse<bool>.SuccessResponse(isEnabled));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if feature {Name} is enabled for role {Role}", name, roleName);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error checking feature status"));
            }
        }

        /// <summary>
        /// Create a new feature toggle
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<FeatureToggleDto>>> CreateFeatureToggle([FromBody] CreateFeatureToggleDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var feature = await _featureToggleService.CreateFeatureToggleAsync(dto, userId);
                return CreatedAtAction(nameof(GetFeatureToggleById), new { id = feature.Id },
                    ApiResponse<FeatureToggleDto>.SuccessResponse(feature, "Feature toggle created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<FeatureToggleDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feature toggle");
                return StatusCode(500, ApiResponse<FeatureToggleDto>.ErrorResponse("Error creating feature toggle"));
            }
        }

        /// <summary>
        /// Update an existing feature toggle
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateFeatureToggle(int id, [FromBody] UpdateFeatureToggleDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _featureToggleService.UpdateFeatureToggleAsync(id, dto, userId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Feature toggle not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Feature toggle updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feature toggle {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating feature toggle"));
            }
        }

        /// <summary>
        /// Toggle a feature on/off with role-based access
        /// </summary>
        [HttpPost("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleFeature(int id, [FromBody] ToggleFeatureDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _featureToggleService.ToggleFeatureAsync(id, dto, userId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Feature toggle not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Feature toggle status changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling feature {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error toggling feature"));
            }
        }

        /// <summary>
        /// Delete a feature toggle
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFeatureToggle(int id)
        {
            try
            {
                var result = await _featureToggleService.DeleteFeatureToggleAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Feature toggle not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Feature toggle deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature toggle {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting feature toggle"));
            }
        }
    }
}
