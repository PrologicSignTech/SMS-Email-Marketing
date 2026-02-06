/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing landing page configuration (admin only)
/// </summary>
public class LandingPageConfigController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LandingPageConfigController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LandingPageConfigController(IApiClient apiClient, ILogger<LandingPageConfigController> logger, IHttpContextAccessor httpContextAccessor)
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
    /// Landing page configuration dashboard
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Configure hero section (banner/slider)
    /// </summary>
    public IActionResult HeroSection()
    {
        return View();
    }

    /// <summary>
    /// Configure navigation menu
    /// </summary>
    public IActionResult MenuConfig()
    {
        return View();
    }

    /// <summary>
    /// Configure features section
    /// </summary>
    public IActionResult Features()
    {
        return View();
    }

    /// <summary>
    /// Configure pricing plans display
    /// </summary>
    public IActionResult PricingDisplay()
    {
        return View();
    }

    /// <summary>
    /// Configure footer
    /// </summary>
    public IActionResult Footer()
    {
        return View();
    }

    /// <summary>
    /// Preview landing page with current settings
    /// </summary>
    public IActionResult Preview()
    {
        return View();
    }

    /// <summary>
    /// Get landing page configuration via server-side API call
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/landingpageconfig");
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to load landing page configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading landing page configuration");
            return Json(new { success = false, message = "An error occurred while loading configuration" });
        }
    }

    /// <summary>
    /// Update landing page configuration via server-side API call
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateConfig([FromBody] object configData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/landingpageconfig", configData);
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = "Configuration updated successfully" });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to update configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating landing page configuration");
            return Json(new { success = false, message = "An error occurred while updating configuration" });
        }
    }
}
