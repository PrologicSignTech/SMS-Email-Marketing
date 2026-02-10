using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for suppression list and opt-out management
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class SuppressionController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SuppressionController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuppressionController(
        IApiClient apiClient,
        ILogger<SuppressionController> logger,
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
    /// Display suppression lists
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get suppression lists data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetSuppressionList([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/suppressionlists?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=name&sortDescending=false";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrEmpty(request.Type))
                queryString += $"&type={request.Type}";

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
            _logger.LogError(ex, "Error fetching suppression lists data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load suppression lists"
            });
        }
    }

    /// <summary>
    /// Delete suppression list (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/suppressionlists/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting suppression list {ListId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create suppression list (GET - show form)
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Create single suppression entry (POST - save to API)
    /// Routes browser call through Web controller to API
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody] object entryData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/suppressionlists", entryData
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Entry added" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating suppression entry");
            return Json(new { success = false, message = "An error occurred while creating the entry" });
        }
    }

    /// <summary>
    /// Bulk create suppression entries (POST - save to API)
    /// Routes browser call through Web controller to API
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> BulkCreateEntries([FromBody] object bulkData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/suppressionlists/bulk", bulkData
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Entries added" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk suppression entries");
            return Json(new { success = false, message = "An error occurred while creating entries" });
        }
    }

    /// <summary>
    /// Manage suppression list entries
    /// </summary>
    public IActionResult Entries(int id)
    {
        ViewBag.ListId = id;
        return View();
    }

    /// <summary>
    /// Import suppression list
    /// </summary>
    public IActionResult Import()
    {
        return View();
    }
}
