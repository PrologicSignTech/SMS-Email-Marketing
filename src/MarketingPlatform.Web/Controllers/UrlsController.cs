using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for URL tracking and link shortening
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class UrlsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<UrlsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlsController(
        IApiClient apiClient,
        ILogger<UrlsController> logger,
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
    /// Display tracked URLs
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get URLs data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetUrls([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/urls",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "createdAt",
                    sortDirection = "desc"
                }
            );

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                var items = dataObj?.GetProperty("items");
                var totalCount = dataObj?.GetProperty("totalCount").GetInt32() ?? 0;

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = items
                });
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
            _logger.LogError(ex, "Error fetching URLs data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load URLs"
            });
        }
    }

    /// <summary>
    /// Delete URL (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/urls/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL {UrlId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create short URL
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load campaigns for URL tracking association
            var campaigns = await _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns");
            ViewBag.Campaigns = campaigns?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading URL creation data");
            TempData["Error"] = "Failed to load required data for URL creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// URL analytics
    /// </summary>
    public IActionResult Analytics(int id)
    {
        ViewBag.UrlId = id;
        return View();
    }
}
