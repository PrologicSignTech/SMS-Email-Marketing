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
    public async Task<IActionResult> GetTemplates([FromBody] DataTablesRequest request)
    {
        try
        {
            var type = request.Status; // type filter from tabs
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/templates",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "lastModified",
                    sortDirection = "desc",
                    type = type
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
    /// Create new template
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load template categories and available merge variables
            var categoriesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates/categories");
            var variablesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates/variables");

            await Task.WhenAll(categoriesTask, variablesTask);

            var categories = await categoriesTask;
            var variables = await variablesTask;

            ViewBag.Categories = categories?.Data;
            ViewBag.Variables = variables?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template creation data");
            TempData["Error"] = "Failed to load required data for template creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit existing template
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var templateTask = _apiClient.GetAsync<ApiResponse<object>>($"/api/templates/{id}");
            var categoriesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates/categories");
            var variablesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates/variables");

            await Task.WhenAll(templateTask, categoriesTask, variablesTask);

            var template = await templateTask;
            var categories = await categoriesTask;
            var variables = await variablesTask;

            if (template?.Success == true && template.Data != null)
            {
                ViewBag.TemplateId = id;
                ViewBag.Categories = categories?.Data;
                ViewBag.Variables = variables?.Data;
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
}
