using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing landing page features
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class LandingFeaturesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LandingFeaturesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LandingFeaturesController(
        IApiClient apiClient,
        ILogger<LandingFeaturesController> logger,
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
    /// Display landing features list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get landing features data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetFeatures([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/landingfeatures");

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;

                if (dataObj.HasValue)
                {
                    var items = dataObj.Value;

                    return Json(new
                    {
                        draw = request.Draw,
                        recordsTotal = items.GetArrayLength(),
                        recordsFiltered = items.GetArrayLength(),
                        data = items
                    });
                }
            }

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching landing features data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load landing features"
            });
        }
    }

    /// <summary>
    /// Delete landing feature (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/landingfeatures/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting landing feature {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new landing feature page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load color classes and icon options if needed
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading feature creation data");
            TempData["Error"] = "Failed to load required data for feature creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit landing feature page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var feature = await _apiClient.GetAsync<ApiResponse<object>>($"/api/landingfeatures/{id}");

            if (feature?.Success == true && feature.Data != null)
            {
                ViewBag.FeatureId = id;
                return View(feature.Data);
            }

            TempData["Error"] = "Feature not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading landing feature {Id}", id);
            TempData["Error"] = "Failed to load feature";
            return RedirectToAction(nameof(Index));
        }
    }
}
