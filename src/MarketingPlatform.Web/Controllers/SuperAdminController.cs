/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for super admin functions
/// </summary>
public class SuperAdminController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SuperAdminController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuperAdminController(IApiClient apiClient, ILogger<SuperAdminController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _apiClient.SetAuthorizationToken(token);
        }
    }

    /// <summary>
    /// Super admin dashboard
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Load dashboard statistics
            var stats = await _apiClient.GetAsync<ApiResponse<object>>("/api/superadmin/stats");
            ViewBag.Stats = stats?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading super admin dashboard");
            TempData["Error"] = "Failed to load dashboard data";
            return View();
        }
    }

    /// <summary>
    /// Manage super admins
    /// </summary>
    public IActionResult Users()
    {
        // User list loaded via DataTables server-side
        return View();
    }

    /// <summary>
    /// Platform configuration
    /// </summary>
    public async Task<IActionResult> PlatformConfig()
    {
        try
        {
            // Load platform configuration
            var config = await _apiClient.GetAsync<ApiResponse<object>>("/api/superadmin/config");
            if (config?.Success == true && config.Data != null)
            {
                return View(config.Data);
            }
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading platform configuration");
            TempData["Error"] = "Failed to load platform configuration";
            return View();
        }
    }

    /// <summary>
    /// Audit logs
    /// </summary>
    public IActionResult AuditLogs()
    {
        // Audit logs loaded via DataTables server-side
        return View();
    }

    /// <summary>
    /// Get platform statistics via server-side API call
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPlatformStats()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/superadmin/stats");
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to load platform statistics" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading platform statistics");
            return Json(new { success = false, message = "An error occurred while loading statistics" });
        }
    }

    /// <summary>
    /// Get audit logs via server-side API call
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/superadmin/auditlogs");
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to load audit logs" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit logs");
            return Json(new { success = false, message = "An error occurred while loading audit logs" });
        }
    }

    /// <summary>
    /// Get platform configuration via server-side API call
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPlatformConfig()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/superadmin/config");
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to load platform configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading platform configuration");
            return Json(new { success = false, message = "An error occurred while loading configuration" });
        }
    }

    /// <summary>
    /// Update platform configuration via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdatePlatformConfig([FromBody] object configData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/superadmin/config", configData);
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = "Configuration updated successfully" });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to update configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating platform configuration");
            return Json(new { success = false, message = "An error occurred while updating configuration" });
        }
    }

    /// <summary>
    /// Get users list via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/users");
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, data = new object[] { }, message = result?.Message ?? "No users found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            return Json(new { success = false, data = new object[] { }, message = "An error occurred while loading users" });
        }
    }

    /// <summary>
    /// Create user via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] object userData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/users", userData);
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = "User created successfully" });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to create user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Json(new { success = false, message = "An error occurred while creating user" });
        }
    }

    /// <summary>
    /// Toggle user status via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/users/{id}/toggle-status", new { });
            if (result?.Success == true)
            {
                return Json(new { success = true, message = "User status updated successfully" });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to update user status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            return Json(new { success = false, message = "An error occurred while updating user status" });
        }
    }

    /// <summary>
    /// Clear cache via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/superadmin/clear-cache", new { });
            return Json(new { success = true, message = "Cache cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Json(new { success = false, message = "An error occurred while clearing cache" });
        }
    }
}
