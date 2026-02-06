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
    public async Task<IActionResult> GetProviders([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/providers",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "name",
                    sortDirection = "asc",
                    type = request.Type
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
    /// Create new provider
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load channel types for dropdown (SMS, Email, WhatsApp, etc.)
            var channelTypes = await _apiClient.GetAsync<ApiResponse<object>>("/api/providers/channeltypes");
            ViewBag.ChannelTypes = channelTypes?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provider creation data");
            TempData["Error"] = "Failed to load required data for provider creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit provider configuration
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var providerTask = _apiClient.GetAsync<ApiResponse<object>>($"/api/providers/{id}");
            var channelTypesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/providers/channeltypes");

            await Task.WhenAll(providerTask, channelTypesTask);

            var provider = await providerTask;
            var channelTypes = await channelTypesTask;

            if (provider?.Success == true && provider.Data != null)
            {
                ViewBag.ProviderId = id;
                ViewBag.ChannelTypes = channelTypes?.Data;
                return View(provider.Data);
            }

            TempData["Error"] = "Provider not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provider {ProviderId}", id);
            TempData["Error"] = "Failed to load provider";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Provider health monitoring
    /// </summary>
    public IActionResult Health()
    {
        return View();
    }

    /// <summary>
    /// Routing configuration
    /// </summary>
    public IActionResult Routing()
    {
        return View();
    }
}
