using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for SMS keyword campaigns management
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class KeywordsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<KeywordsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KeywordsController(
        IApiClient apiClient,
        ILogger<KeywordsController> logger,
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
    /// Display keywords list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get keywords data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetKeywords([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/keywords",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "createdAt",
                    sortDirection = "desc",
                    isActive = request.IsActive
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
            _logger.LogError(ex, "Error fetching keywords data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load keywords"
            });
        }
    }

    /// <summary>
    /// Delete keyword (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/keywords/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting keyword {KeywordId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new keyword
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load phone numbers/providers for dropdown
            var providers = await _apiClient.GetAsync<ApiResponse<object>>("/api/providers");
            ViewBag.Providers = providers?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading keyword creation data");
            TempData["Error"] = "Failed to load required data for keyword creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit keyword
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var keywordTask = _apiClient.GetAsync<ApiResponse<object>>($"/api/keywords/{id}");
            var providersTask = _apiClient.GetAsync<ApiResponse<object>>("/api/providers");

            await Task.WhenAll(keywordTask, providersTask);

            var keyword = await keywordTask;
            var providers = await providersTask;

            if (keyword?.Success == true && keyword.Data != null)
            {
                ViewBag.KeywordId = id;
                ViewBag.Providers = providers?.Data;
                return View(keyword.Data);
            }

            TempData["Error"] = "Keyword not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading keyword {KeywordId}", id);
            TempData["Error"] = "Failed to load keyword";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Keyword analytics
    /// </summary>
    public IActionResult Analytics(int id)
    {
        ViewBag.KeywordId = id;
        return View();
    }
}
