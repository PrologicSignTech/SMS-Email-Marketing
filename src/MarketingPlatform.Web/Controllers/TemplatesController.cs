using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing message templates
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class TemplatesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TemplatesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplatesController(
        IApiClient apiClient,
        ILogger<TemplatesController> logger,
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
    /// Display templates list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get templates data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetTemplates([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";

            // If filtering by channel, use the channel-specific endpoint
            if (request.Channel.HasValue)
            {
                try
                {
                    var channelResult = await _apiClient.GetAsync<ApiResponse<object>>($"/api/templates/channel/{request.Channel.Value}");
                    if (channelResult?.Success == true)
                    {
                        var channelData = channelResult.Data as System.Text.Json.JsonElement?;
                        var allItems = new List<System.Text.Json.JsonElement>();

                        if (channelData?.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            allItems = channelData.Value.EnumerateArray().ToList();
                        }
                        else if (channelData?.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            // Might be a paginated result
                            try
                            {
                                var items = channelData.Value.GetProperty("items");
                                allItems = items.EnumerateArray().ToList();
                            }
                            catch
                            {
                                allItems = channelData.Value.EnumerateArray().ToList();
                            }
                        }

                        // Client-side search
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            allItems = allItems.Where(item =>
                            {
                                try
                                {
                                    var name = item.GetProperty("name").GetString() ?? "";
                                    return name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                                }
                                catch { return false; }
                            }).ToList();
                        }

                        var totalFiltered = allItems.Count;
                        var pagedItems = allItems.Skip(request.Start).Take(pageSize).ToList();

                        return Json(new
                        {
                            draw = request.Draw,
                            recordsTotal = totalFiltered,
                            recordsFiltered = totalFiltered,
                            data = pagedItems
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Channel filter failed, falling back to default query");
                }
            }

            var queryString = $"/api/templates?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=updatedAt&sortDescending=true";
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
            _logger.LogError(ex, "Error fetching templates data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load templates"
            });
        }
    }

    /// <summary>
    /// Duplicate template (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/templates/{id}/duplicate",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating template {TemplateId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete template (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/templates/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new template (GET - show form)
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Create new template (POST - save to API)
    /// </summary>
    [HttpPost("Templates/CreateTemplate")]
    public async Task<IActionResult> CreateTemplate([FromBody] object templateData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/templates", templateData
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Template created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return Json(new { success = false, message = "An error occurred while creating the template" });
        }
    }

    /// <summary>
    /// Edit existing template (GET - show form)
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var template = await _apiClient.GetAsync<ApiResponse<object>>($"/api/templates/{id}");

            if (template?.Success == true && template.Data != null)
            {
                ViewBag.TemplateId = id;
                return View(template.Data);
            }

            TempData["Error"] = "Template not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template {TemplateId}", id);
            TempData["Error"] = "Failed to load template";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Get template by ID (for Edit and Preview pages)
    /// Routes through Web controller instead of direct API call from browser
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/templates/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data, message = result?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template {TemplateId}", id);
            return Json(new { success = false, message = "Failed to load template" });
        }
    }

    /// <summary>
    /// Preview template (GET - show preview page)
    /// </summary>
    public async Task<IActionResult> Preview(int id)
    {
        try
        {
            var template = await _apiClient.GetAsync<ApiResponse<object>>($"/api/templates/{id}");

            if (template?.Success == true && template.Data != null)
            {
                ViewBag.TemplateId = id;
                return View(template.Data);
            }

            TempData["Error"] = "Template not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template preview {TemplateId}", id);
            TempData["Error"] = "Failed to load template";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Update template (POST - save to API)
    /// </summary>
    [HttpPost("Templates/UpdateTemplate")]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] object templateData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>(
                $"/api/templates/{id}", templateData
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Template updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return Json(new { success = false, message = "An error occurred while updating the template" });
        }
    }
}
