using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing landing page stats
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class LandingStatsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LandingStatsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LandingStatsController(
        IApiClient apiClient,
        ILogger<LandingStatsController> logger,
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
    /// Display landing stats list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get landing stats data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetStats([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/landingstats");

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
            _logger.LogError(ex, "Error fetching landing stats data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load landing stats"
            });
        }
    }

    /// <summary>
    /// Delete landing stat (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/landingstats/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting landing stat {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new landing stat page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Edit landing stat page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var stat = await _apiClient.GetAsync<ApiResponse<object>>($"/api/landingstats/{id}");

            if (stat?.Success == true && stat.Data != null)
            {
                ViewBag.StatId = id;
                return View(stat.Data);
            }

            TempData["Error"] = "Stat not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading landing stat {Id}", id);
            TempData["Error"] = "Failed to load stat";
            return RedirectToAction(nameof(Index));
        }
    }
}
