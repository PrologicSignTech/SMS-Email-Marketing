using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for provider management (Super Admin)
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class ProvidersController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ProvidersController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProvidersController(
        IApiClient apiClient,
        ILogger<ProvidersController> logger,
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
    /// Display providers list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get providers data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetProviders([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            // API endpoint is [HttpGet] with [FromQuery] PagedRequest
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";

            var queryString = $"/api/providers?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=name&sortDescending=false";
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
            _logger.LogError(ex, "Error fetching providers data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load providers"
            });
        }
    }

    /// <summary>
    /// Test provider (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Test(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/providers/{id}/test",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider {ProviderId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete provider (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/providers/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider {ProviderId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new provider page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Save new provider (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProvider([FromBody] object providerData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/providers", providerData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Provider created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Edit provider page
    /// </summary>
    public IActionResult Edit(int id)
    {
        ViewBag.ProviderId = id;
        return View();
    }

    /// <summary>
    /// Get provider details for edit form (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProvider(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/providers/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching provider {ProviderId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update provider (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateProvider(int id, [FromBody] object providerData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/providers/{id}", providerData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Provider updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider {ProviderId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
