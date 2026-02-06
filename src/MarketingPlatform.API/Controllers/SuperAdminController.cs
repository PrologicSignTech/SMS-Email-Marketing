using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Application.DTOs.Analytics;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;
        private readonly IPrivilegedActionLogService _logService;
        private readonly IPlatformConfigurationService _configService;
        private readonly ISuperAdminAnalyticsService _analyticsService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            ISuperAdminService superAdminService,
            IPrivilegedActionLogService logService,
            IPlatformConfigurationService configService,
            ISuperAdminAnalyticsService analyticsService,
            ILogger<SuperAdminController> logger)
        {
            _superAdminService = superAdminService;
            _logService = logService;
            _configService = configService;
            _analyticsService = analyticsService;
            _logger = logger;
        }

        #region Super Admin Role Management

        /// <summary>
        /// Get all active super admin assignments
        /// </summary>
        [HttpGet("roles")]
        [Authorize(Policy = "Permission:ManageSuperAdmins")]
        public async Task<ActionResult<ApiResponse<List<SuperAdminRoleDto>>>> GetActiveSuperAdmins()
        {
            try
            {
                var superAdmins = await _superAdminService.GetActiveSuperAdminsAsync();
                return Ok(ApiResponse<List<SuperAdminRoleDto>>.SuccessResponse(superAdmins.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving super admin roles");
                return BadRequest(ApiResponse<List<SuperAdminRoleDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get super admin assignment for a specific user
        /// </summary>
        [HttpGet("roles/{userId}")]
        [Authorize(Policy = "Permission:ManageSuperAdmins")]
        public async Task<ActionResult<ApiResponse<SuperAdminRoleDto>>> GetSuperAdminByUserId(string userId)
        {
            try
            {
                var superAdmin = await _superAdminService.GetSuperAdminByUserIdAsync(userId);
                if (superAdmin == null)
                    return NotFound(ApiResponse<SuperAdminRoleDto>.ErrorResponse("Super admin role not found for this user"));

                return Ok(ApiResponse<SuperAdminRoleDto>.SuccessResponse(superAdmin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving super admin role for user {UserId}", userId);
                return BadRequest(ApiResponse<SuperAdminRoleDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Assign super admin role to a user
        /// </summary>
        [HttpPost("roles/assign")]
        [Authorize(Policy = "Permission:ManageSuperAdmins")]
        public async Task<ActionResult<ApiResponse<SuperAdminRoleDto>>> AssignSuperAdmin([FromBody] AssignSuperAdminDto request)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var superAdmin = await _superAdminService.AssignSuperAdminAsync(currentUserId, request);

                // Log the privileged action
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var requestPath = Request.Path;

                await _logService.LogActionAsync(currentUserId, new CreatePrivilegedActionLogDto
                {
                    ActionType = PrivilegedActionType.SuperAdminAssigned,
                    Severity = ActionSeverity.Critical,
                    EntityType = "User",
                    EntityId = request.UserId,
                    EntityName = superAdmin.UserEmail,
                    ActionDescription = $"Assigned super admin privileges to {superAdmin.UserEmail}",
                    AfterState = System.Text.Json.JsonSerializer.Serialize(superAdmin),
                    Success = true
                }, ipAddress, userAgent, requestPath);

                return CreatedAtAction(nameof(GetSuperAdminByUserId), new { userId = request.UserId },
                    ApiResponse<SuperAdminRoleDto>.SuccessResponse(superAdmin, "Super admin role assigned successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning super admin role");
                return BadRequest(ApiResponse<SuperAdminRoleDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Revoke super admin role from a user
        /// </summary>
        [HttpPost("roles/revoke")]
        [Authorize(Policy = "Permission:ManageSuperAdmins")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeSuperAdmin([FromBody] RevokeSuperAdminDto request)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var result = await _superAdminService.RevokeSuperAdminAsync(currentUserId, request);

                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("No active super admin role found for this user"));

                // Log the privileged action
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var requestPath = Request.Path;

                await _logService.LogActionAsync(currentUserId, new CreatePrivilegedActionLogDto
                {
                    ActionType = PrivilegedActionType.SuperAdminRevoked,
                    Severity = ActionSeverity.Critical,
                    EntityType = "User",
                    EntityId = request.UserId,
                    ActionDescription = $"Revoked super admin privileges from user {request.UserId}",
                    Success = true
                }, ipAddress, userAgent, requestPath);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Super admin role revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking super admin role");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Check if a user is a super admin
        /// </summary>
        [HttpGet("roles/check/{userId}")]
        [Authorize(Policy = "Permission:ManageSuperAdmins")]
        public async Task<ActionResult<ApiResponse<bool>>> IsSuperAdmin(string userId)
        {
            try
            {
                var isSuperAdmin = await _superAdminService.IsSuperAdminAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResponse(isSuperAdmin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking super admin status for user {UserId}", userId);
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Privileged Action Logs

        /// <summary>
        /// Get paginated privileged action logs with filtering
        /// </summary>
        [HttpGet("logs")]
        [Authorize(Policy = "Permission:ViewSuperAdminLogs")]
        public async Task<ActionResult<ApiResponse<object>>> GetPrivilegedActionLogs([FromQuery] PrivilegedActionFilterDto filter)
        {
            try
            {
                var (logs, totalCount) = await _logService.GetPagedLogsAsync(filter);
                
                var response = new
                {
                    Logs = logs,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };

                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving privileged action logs");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get logs for a specific entity
        /// </summary>
        [HttpGet("logs/entity/{entityType}/{entityId}")]
        [Authorize(Policy = "Permission:ViewSuperAdminLogs")]
        public async Task<ActionResult<ApiResponse<List<PrivilegedActionLogDto>>>> GetLogsByEntity(string entityType, string entityId)
        {
            try
            {
                var logs = await _logService.GetLogsByEntityAsync(entityType, entityId);
                return Ok(ApiResponse<List<PrivilegedActionLogDto>>.SuccessResponse(logs.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs for entity {EntityType}:{EntityId}", entityType, entityId);
                return BadRequest(ApiResponse<List<PrivilegedActionLogDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get logs for a specific user
        /// </summary>
        [HttpGet("logs/user/{userId}")]
        [Authorize(Policy = "Permission:ViewSuperAdminLogs")]
        public async Task<ActionResult<ApiResponse<List<PrivilegedActionLogDto>>>> GetLogsByUser(string userId, [FromQuery] int limit = 100)
        {
            try
            {
                var logs = await _logService.GetLogsByUserAsync(userId, limit);
                return Ok(ApiResponse<List<PrivilegedActionLogDto>>.SuccessResponse(logs.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs for user {UserId}", userId);
                return BadRequest(ApiResponse<List<PrivilegedActionLogDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get critical actions within a time range
        /// </summary>
        [HttpGet("logs/critical")]
        [Authorize(Policy = "Permission:ViewSuperAdminLogs")]
        public async Task<ActionResult<ApiResponse<List<PrivilegedActionLogDto>>>> GetCriticalActions(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var logs = await _logService.GetCriticalActionsAsync(startDate, endDate);
                return Ok(ApiResponse<List<PrivilegedActionLogDto>>.SuccessResponse(logs.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving critical actions");
                return BadRequest(ApiResponse<List<PrivilegedActionLogDto>>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Platform Configuration

        /// <summary>
        /// Get all active platform configurations
        /// </summary>
        [HttpGet("config")]
        [Authorize(Policy = "Permission:ViewPlatformConfig")]
        public async Task<ActionResult<ApiResponse<List<PlatformConfigurationDto>>>> GetActiveConfigurations()
        {
            try
            {
                var configs = await _configService.GetActiveConfigurationsAsync();
                return Ok(ApiResponse<List<PlatformConfigurationDto>>.SuccessResponse(configs.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform configurations");
                return BadRequest(ApiResponse<List<PlatformConfigurationDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get configuration by key
        /// </summary>
        [HttpGet("config/{key}")]
        [Authorize(Policy = "Permission:ViewPlatformConfig")]
        public async Task<ActionResult<ApiResponse<PlatformConfigurationDto>>> GetConfigurationByKey(string key)
        {
            try
            {
                var config = await _configService.GetByKeyAsync(key);
                if (config == null)
                    return NotFound(ApiResponse<PlatformConfigurationDto>.ErrorResponse("Configuration not found"));

                return Ok(ApiResponse<PlatformConfigurationDto>.SuccessResponse(config));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration {Key}", key);
                return BadRequest(ApiResponse<PlatformConfigurationDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get configurations by category
        /// </summary>
        [HttpGet("config/category/{category}")]
        [Authorize(Policy = "Permission:ViewPlatformConfig")]
        public async Task<ActionResult<ApiResponse<List<PlatformConfigurationDto>>>> GetConfigurationsByCategory(ConfigurationCategory category)
        {
            try
            {
                var configs = await _configService.GetByCategoryAsync(category);
                return Ok(ApiResponse<List<PlatformConfigurationDto>>.SuccessResponse(configs.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configurations for category {Category}", category);
                return BadRequest(ApiResponse<List<PlatformConfigurationDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update a platform configuration
        /// </summary>
        [HttpPut("config")]
        [Authorize(Policy = "Permission:ManagePlatformConfig")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateConfiguration([FromBody] UpdateConfigurationDto request)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Get the old configuration for audit trail
                var oldConfig = await _configService.GetByKeyAsync(request.Key);
                var beforeState = oldConfig != null ? System.Text.Json.JsonSerializer.Serialize(oldConfig) : null;

                var result = await _configService.UpdateConfigurationAsync(currentUserId, request);

                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Configuration not found"));

                // Get the updated configuration for audit trail
                var newConfig = await _configService.GetByKeyAsync(request.Key);
                var afterState = newConfig != null ? System.Text.Json.JsonSerializer.Serialize(newConfig) : null;

                // Log the privileged action
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var requestPath = Request.Path;

                await _logService.LogActionAsync(currentUserId, new CreatePrivilegedActionLogDto
                {
                    ActionType = PrivilegedActionType.PlatformConfigurationUpdated,
                    Severity = ActionSeverity.High,
                    EntityType = "PlatformConfiguration",
                    EntityId = request.Key,
                    EntityName = request.Key,
                    ActionDescription = $"Updated platform configuration: {request.Key}",
                    BeforeState = beforeState,
                    AfterState = afterState,
                    Success = true
                }, ipAddress, userAgent, requestPath);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Configuration updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Toggle a platform feature
        /// </summary>
        [HttpPost("config/toggle-feature")]
        [Authorize(Policy = "Permission:ManagePlatformConfig")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleFeature(
            [FromQuery] PlatformFeature feature,
            [FromQuery] bool enabled)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var result = await _configService.ToggleFeatureAsync(currentUserId, feature, enabled);

                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Feature configuration not found"));

                // Log the privileged action
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var requestPath = Request.Path;

                await _logService.LogActionAsync(currentUserId, new CreatePrivilegedActionLogDto
                {
                    ActionType = PrivilegedActionType.FeatureToggled,
                    Severity = ActionSeverity.High,
                    EntityType = "PlatformFeature",
                    EntityId = feature.ToString(),
                    EntityName = feature.ToString(),
                    ActionDescription = $"Toggled feature {feature} to {(enabled ? "enabled" : "disabled")}",
                    AfterState = enabled.ToString(),
                    Success = true
                }, ipAddress, userAgent, requestPath);

                return Ok(ApiResponse<bool>.SuccessResponse(true, $"Feature {feature} {(enabled ? "enabled" : "disabled")} successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling feature");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Analytics & Monitoring

        /// <summary>
        /// Get platform-wide analytics
        /// </summary>
        [HttpGet("analytics/platform")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<PlatformAnalyticsDto>>> GetPlatformAnalytics()
        {
            try
            {
                var analytics = await _analyticsService.GetPlatformAnalyticsAsync();
                return Ok(ApiResponse<PlatformAnalyticsDto>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform analytics");
                return BadRequest(ApiResponse<PlatformAnalyticsDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get billing analytics
        /// </summary>
        [HttpGet("analytics/billing")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<BillingAnalyticsDto>>> GetBillingAnalytics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetBillingAnalyticsAsync(startDate, endDate);
                return Ok(ApiResponse<BillingAnalyticsDto>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing analytics");
                return BadRequest(ApiResponse<BillingAnalyticsDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get monthly revenue trends
        /// </summary>
        [HttpGet("analytics/revenue/monthly")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<List<MonthlyRevenueDto>>>> GetMonthlyRevenue([FromQuery] int months = 12)
        {
            try
            {
                var revenue = await _analyticsService.GetMonthlyRevenueAsync(months);
                return Ok(ApiResponse<List<MonthlyRevenueDto>>.SuccessResponse(revenue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly revenue");
                return BadRequest(ApiResponse<List<MonthlyRevenueDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        [HttpGet("health")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<SystemHealthDto>>> GetSystemHealth()
        {
            try
            {
                var health = await _analyticsService.GetSystemHealthAsync();
                return Ok(ApiResponse<SystemHealthDto>.SuccessResponse(health));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health");
                return BadRequest(ApiResponse<SystemHealthDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get provider health metrics
        /// </summary>
        [HttpGet("health/providers")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProviderHealthDto>>>> GetProviderHealth()
        {
            try
            {
                var providerHealth = await _analyticsService.GetProviderHealthAsync();
                return Ok(ApiResponse<List<ProviderHealthDto>>.SuccessResponse(providerHealth));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider health");
                return BadRequest(ApiResponse<List<ProviderHealthDto>>.ErrorResponse(ex.Message));
            }
        }

        #endregion
    }
}
