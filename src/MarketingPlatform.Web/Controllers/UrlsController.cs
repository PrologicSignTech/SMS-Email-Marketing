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
    public async Task<IActionResult> GetUrls([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/urls?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            var result = await _apiClient.GetAsync<ApiResponse<object>>(queryString);

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
    /// Create short URL page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Get campaigns for dropdown (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaigns()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns?pageNumber=1&pageSize=200");
            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && dataObj.Value.TryGetProperty("items", out var itemsElement))
                {
                    return Json(new { success = true, items = itemsElement });
                }
                return Json(new { success = true, items = result.Data });
            }
            return Json(new { success = false, items = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns");
            return Json(new { success = false, items = new object[] { } });
        }
    }

    /// <summary>
    /// Save new URL (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUrl([FromBody] object urlData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/urls", urlData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "URL created", data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating URL");
            return Json(new { success = false, message = "An error occurred" });
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
