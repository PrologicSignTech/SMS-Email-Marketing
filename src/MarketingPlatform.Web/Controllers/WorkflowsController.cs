using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing automated workflows and journeys
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class WorkflowsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<WorkflowsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkflowsController(
        IApiClient apiClient,
        ILogger<WorkflowsController> logger,
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
    /// Display workflows list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get workflows data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetWorkflows([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/workflows",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "createdAt",
                    sortDirection = "desc"
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
            _logger.LogError(ex, "Error fetching workflows data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load workflows"
            });
        }
    }

    /// <summary>
    /// Activate workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/workflows/{id}/activate",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Pause workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Pause(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/workflows/{id}/pause",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/workflows/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new workflow page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load reference data needed for workflow creation
            var contactGroupsTask = _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");
            var templatesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates");
            var campaignsTask = _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns");

            await Task.WhenAll(contactGroupsTask, templatesTask, campaignsTask);

            var contactGroups = await contactGroupsTask;
            var templates = await templatesTask;
            var campaigns = await campaignsTask;

            ViewBag.ContactGroups = contactGroups?.Data;
            ViewBag.Templates = templates?.Data;
            ViewBag.Campaigns = campaigns?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow creation data");
            TempData["Error"] = "Failed to load required data for workflow creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit workflow page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var workflow = await _apiClient.GetAsync<ApiResponse<object>>(
                $"/api/workflows/{id}"
            );

            if (workflow?.Success == true && workflow.Data != null)
            {
                ViewBag.WorkflowId = id;
                return View(workflow.Data);
            }

            TempData["Error"] = "Workflow not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow {WorkflowId}", id);
            TempData["Error"] = "Failed to load workflow";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// View workflow details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var workflow = await _apiClient.GetAsync<ApiResponse<object>>(
                $"/api/workflows/{id}"
            );

            if (workflow?.Success == true && workflow.Data != null)
            {
                return View(workflow.Data);
            }

            TempData["Error"] = "Workflow not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow {WorkflowId}", id);
            TempData["Error"] = "Failed to load workflow";
            return RedirectToAction(nameof(Index));
        }
    }
}
