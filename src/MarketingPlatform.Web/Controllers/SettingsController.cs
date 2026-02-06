using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for platform settings and configuration
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class SettingsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SettingsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SettingsController(
        IApiClient apiClient,
        ILogger<SettingsController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        // Set authorization token from user's access_token claim
        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _apiClient.SetAuthorizationToken(token);
        }
    }

    /// <summary>
    /// Display settings page
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get settings (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/settings");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load settings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings");
            return Json(new { success = false, message = "An error occurred while loading settings" });
        }
    }

    /// <summary>
    /// Update settings (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] object settings)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>("/api/settings", settings);

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Integration settings
    /// </summary>
    public IActionResult Integrations()
    {
        return View();
    }

    /// <summary>
    /// Get integration settings (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetIntegrationSettings()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/settings/integrations");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load integration settings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching integration settings");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Compliance settings
    /// </summary>
    public IActionResult Compliance()
    {
        return View();
    }

    /// <summary>
    /// Get compliance settings (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetComplianceSettings()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/settings/compliance");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load compliance settings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching compliance settings");
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
