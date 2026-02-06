using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// API Management Controller - For SuperAdmins to view and test APIs
/// Similar to Swagger UI but integrated in the admin panel
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class ApiManagementController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ApiManagementController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiManagementController(
        IApiClient apiClient,
        ILogger<ApiManagementController> logger,
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
    /// API Management Dashboard - Main page for API documentation and testing
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get API documentation (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetApiDocs()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/apidocs");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load API documentation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching API documentation");
            return Json(new { success = false, message = "An error occurred while loading API documentation" });
        }
    }

    /// <summary>
    /// API Testing Console - Interactive console for testing endpoints
    /// </summary>
    public IActionResult Console()
    {
        return View();
    }

    /// <summary>
    /// API Documentation - Detailed docs for all endpoints
    /// </summary>
    public IActionResult Documentation()
    {
        return View();
    }
}
