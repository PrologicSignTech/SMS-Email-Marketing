using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Campaigns Controller - SERVER-SIDE API INTEGRATION
/// </summary>
[Authorize]
public class CampaignsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<CampaignsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CampaignsController(
        IApiClient apiClient,
        ILogger<CampaignsController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _apiClient.SetAuthorizationToken(token);
        }
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetCampaigns([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/campaigns",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = request.Order?[0]?.Column ?? "createdAt",
                    sortDirection = request.Order?[0]?.Dir ?? "desc",
                    status = request.Status
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

            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load campaigns" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/campaigns/{id}/duplicate", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/campaigns/{id}/start", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Pause(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/campaigns/{id}/pause", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Resume(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/campaigns/{id}/resume", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/campaigns/{id}/cancel", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/campaigns/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            // Load reference data needed for campaign creation
            var templatesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates");
            var contactGroupsTask = _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");
            var providersTask = _apiClient.GetAsync<ApiResponse<object>>("/api/providers");

            await Task.WhenAll(templatesTask, contactGroupsTask, providersTask);

            var templates = await templatesTask;
            var contactGroups = await contactGroupsTask;
            var providers = await providersTask;

            ViewBag.Templates = templates?.Data;
            ViewBag.ContactGroups = contactGroups?.Data;
            ViewBag.Providers = providers?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaign creation data");
            TempData["Error"] = "Failed to load required data for campaign creation";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var campaign = await _apiClient.GetAsync<ApiResponse<object>>($"/api/campaigns/{id}");
            if (campaign?.Success == true && campaign.Data != null)
            {
                return View(campaign.Data);
            }
            TempData["Error"] = "Campaign not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaign {CampaignId}", id);
            TempData["Error"] = "Failed to load campaign";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Variants(int id)
    {
        ViewBag.CampaignId = id;
        return View();
    }
}
