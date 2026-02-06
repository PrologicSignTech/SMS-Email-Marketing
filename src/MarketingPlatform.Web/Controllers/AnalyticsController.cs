using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for analytics and reporting
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class AnalyticsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<AnalyticsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AnalyticsController(
        IApiClient apiClient,
        ILogger<AnalyticsController> logger,
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
    /// Display analytics dashboard
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get dashboard analytics data (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/dashboard");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load analytics data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics dashboard data");
            return Json(new { success = false, message = "An error occurred while loading analytics" });
        }
    }

    /// <summary>
    /// Campaign performance report
    /// </summary>
    public IActionResult Campaigns()
    {
        return View();
    }

    /// <summary>
    /// Get campaign analytics data (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignAnalytics()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/campaigns");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load campaign analytics" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign analytics data");
            return Json(new { success = false, message = "An error occurred while loading campaign analytics" });
        }
    }

    /// <summary>
    /// Export reports
    /// </summary>
    public IActionResult Reports()
    {
        return View();
    }
}
