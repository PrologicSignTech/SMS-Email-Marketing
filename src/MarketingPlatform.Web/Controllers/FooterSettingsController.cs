using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing footer settings
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class FooterSettingsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<FooterSettingsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FooterSettingsController(
        IApiClient apiClient,
        ILogger<FooterSettingsController> logger,
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
    /// Display footer settings management page
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get footer settings data (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/footersettings");

            if (result?.Success == true && result.Data != null)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = "No footer settings found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching footer settings");
            return Json(new { success = false, message = "Failed to load footer settings" });
        }
    }

    /// <summary>
    /// Save footer settings (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] object settings)
    {
        try
        {
            // Try to get existing settings first
            var existingResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/footersettings");

            ApiResponse<object>? result;
            if (existingResult?.Success == true && existingResult.Data != null)
            {
                // Update existing
                result = await _apiClient.PutAsync<object, ApiResponse<object>>("/api/footersettings", settings);
            }
            else
            {
                // Create new
                result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/footersettings", settings);
            }

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving footer settings");
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
