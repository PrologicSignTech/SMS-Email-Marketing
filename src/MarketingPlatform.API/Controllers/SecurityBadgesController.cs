using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/securitybadges")]
    public class SecurityBadgesController : ControllerBase
    {
        private readonly ISecurityBadgeService _securityBadgeService;
        private readonly ILogger<SecurityBadgesController> _logger;

        public SecurityBadgesController(
            ISecurityBadgeService securityBadgeService,
            ILogger<SecurityBadgesController> logger)
        {
            _securityBadgeService = securityBadgeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active security badges for landing page (Public endpoint)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<SecurityBadge>>>> GetSecurityBadges()
        {
            try
            {
                var badges = await _securityBadgeService.GetAllActiveAsync();
                var badgeList = badges.ToList();

                _logger.LogInformation("Retrieved {Count} security badges for landing page", badgeList.Count);

                return Ok(ApiResponse<List<SecurityBadge>>.SuccessResponse(badgeList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security badges");
                return BadRequest(ApiResponse<List<SecurityBadge>>.ErrorResponse(
                    "Failed to retrieve security badges",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific security badge by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult<ApiResponse<SecurityBadge>>> GetById(int id)
        {
            try
            {
                var badge = await _securityBadgeService.GetByIdAsync(id);

                if (badge == null)
                    return NotFound(ApiResponse<SecurityBadge>.ErrorResponse("Security badge not found"));

                return Ok(ApiResponse<SecurityBadge>.SuccessResponse(badge));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security badge {BadgeId}", id);
                return BadRequest(ApiResponse<SecurityBadge>.ErrorResponse(
                    "Failed to retrieve security badge",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new security badge (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult<ApiResponse<SecurityBadge>>> Create([FromBody] SecurityBadge badge)
        {
            try
            {
                var createdBadge = await _securityBadgeService.CreateAsync(badge);

                return Ok(ApiResponse<SecurityBadge>.SuccessResponse(
                    createdBadge,
                    "Security badge created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security badge");
                return BadRequest(ApiResponse<SecurityBadge>.ErrorResponse(
                    "Failed to create security badge",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a security badge (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult<ApiResponse<SecurityBadge>>> Update(int id, [FromBody] SecurityBadge badge)
        {
            try
            {
                var updatedBadge = await _securityBadgeService.UpdateAsync(id, badge);

                return Ok(ApiResponse<SecurityBadge>.SuccessResponse(
                    updatedBadge,
                    "Security badge updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<SecurityBadge>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security badge {BadgeId}", id);
                return BadRequest(ApiResponse<SecurityBadge>.ErrorResponse(
                    "Failed to update security badge",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a security badge (SuperAdmin only - soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _securityBadgeService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(
                    result,
                    "Security badge deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting security badge {BadgeId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete security badge",
                    new List<string> { ex.Message }));
            }
        }
    }
}
