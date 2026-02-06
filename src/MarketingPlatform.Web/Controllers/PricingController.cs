using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing pricing plans and models
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class PricingController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<PricingController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PricingController(
        IApiClient apiClient,
        ILogger<PricingController> logger,
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
    /// Display pricing plans management (Admin only)
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get pricing plans for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetPricingPlans([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/pricing",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value
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
            _logger.LogError(ex, "Error fetching pricing plans data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load pricing plans"
            });
        }
    }

    /// <summary>
    /// Delete pricing plan (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/pricing/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pricing plan {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new pricing plan
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load available channels for pricing plan
            var channels = await _apiClient.GetAsync<ApiResponse<object>>("/api/pricing/channels");
            ViewBag.Channels = channels?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing plan creation data");
            TempData["Error"] = "Failed to load required data for pricing plan creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit pricing plan
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var plan = await _apiClient.GetAsync<ApiResponse<object>>($"/api/pricing/{id}");

            if (plan?.Success == true && plan.Data != null)
            {
                ViewBag.PricingModelId = id;
                return View(plan.Data);
            }

            TempData["Error"] = "Pricing plan not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing plan {Id}", id);
            TempData["Error"] = "Failed to load pricing plan";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Manage channel pricing
    /// </summary>
    public async Task<IActionResult> Channels(int modelId)
    {
        try
        {
            var plan = await _apiClient.GetAsync<ApiResponse<object>>($"/api/pricing/{modelId}");

            if (plan?.Success == true && plan.Data != null)
            {
                ViewBag.PricingModelId = modelId;
                return View(plan.Data);
            }

            TempData["Error"] = "Pricing model not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing model {ModelId}", modelId);
            TempData["Error"] = "Failed to load pricing model";
            return RedirectToAction(nameof(Index));
        }
    }
}
