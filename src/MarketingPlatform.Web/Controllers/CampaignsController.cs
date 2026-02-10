using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;
using System.Text.Json;

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
    public async Task<IActionResult> GetCampaigns([FromBody] DataTablesRequest? request)
    {
        // Defensive: if model binding fails, use defaults
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            // API endpoint is [HttpGet] with [FromQuery] PagedRequest
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var sortBy = request.Order?.FirstOrDefault()?.Column ?? "createdAt";
            var sortDescending = (request.Order?.FirstOrDefault()?.Dir ?? "desc") == "desc";

            // Check if status filter is applied
            if (request.Status.HasValue)
            {
                // Use the status-specific API endpoint
                var statusResult = await _apiClient.GetAsync<ApiResponse<object>>($"/api/campaigns/status/{request.Status.Value}");
                if (statusResult?.Success == true)
                {
                    var statusData = statusResult.Data as System.Text.Json.JsonElement?;
                    // This endpoint returns a flat list, not paginated
                    var allItems = statusData?.EnumerateArray().ToList() ?? new List<System.Text.Json.JsonElement>();
                    var totalCount = allItems.Count;

                    // Apply client-side search if needed
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        allItems = allItems.Where(item =>
                        {
                            var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                            return name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                        }).ToList();
                    }

                    // Apply client-side pagination
                    var pagedItems = allItems.Skip(request.Start).Take(pageSize).ToList();

                    return Json(new
                    {
                        draw = request.Draw,
                        recordsTotal = totalCount,
                        recordsFiltered = allItems.Count,
                        data = pagedItems
                    });
                }

                return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { } });
            }

            var queryString = $"/api/campaigns?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}&sortDescending={sortDescending}";
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

    /// <summary>
    /// Get templates for campaign content dropdown (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/templates?pageNumber=1&pageSize=200");
            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && dataObj.Value.TryGetProperty("items", out var itemsElement))
                {
                    return Json(new { success = true, data = itemsElement });
                }
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching templates for campaign");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get contact groups for audience selection (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactGroups()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups?pageNumber=1&pageSize=200");
            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && dataObj.Value.TryGetProperty("items", out var itemsElement))
                {
                    return Json(new { success = true, data = itemsElement });
                }
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contact groups for campaign");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Create campaign (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] object campaignData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/campaigns", campaignData);
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Campaign created successfully!" });
            }
            return Json(new { success = false, message = result?.Message ?? "Failed to create campaign" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error creating campaign");
            // Pass through the actual API error message
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Calculate audience size (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CalculateAudience([FromBody] object audienceData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/campaigns/calculate-audience", audienceData);
            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, data = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating audience");
            return Json(new { success = false, data = 0 });
        }
    }
}
