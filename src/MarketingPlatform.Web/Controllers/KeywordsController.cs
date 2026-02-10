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
    public async Task<IActionResult> GetKeywords([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/keywords?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
    /// Create new keyword page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Get phone numbers for short code dropdown (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPhoneNumbers()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/phonenumbers?pageNumber=1&pageSize=200");
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
            _logger.LogError(ex, "Error fetching phone numbers");
            return Json(new { success = false, items = new object[] { } });
        }
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
    /// Save new keyword (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateKeyword([FromBody] object keywordData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/keywords", keywordData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Keyword created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating keyword");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Edit keyword page
    /// </summary>
    public IActionResult Edit(int id)
    {
        ViewBag.KeywordId = id;
        return View();
    }

    /// <summary>
    /// Get keyword details (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetKeyword(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/keywords/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching keyword {KeywordId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update keyword (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateKeyword(int id, [FromBody] object keywordData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/keywords/{id}", keywordData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Keyword updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating keyword {KeywordId}", id);
            return Json(new { success = false, message = "An error occurred" });
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
