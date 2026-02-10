using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing automated workflows
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

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
            _apiClient.SetAuthorizationToken(token);
    }

    public IActionResult Index() => View();

    /// <summary>
    /// Get workflows for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetWorkflows([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/workflows?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
            _logger.LogError(ex, "Error fetching workflows data");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load workflows" });
        }
    }

    /// <summary>
    /// Get single workflow by ID
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkflow(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/workflows/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create workflow page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var contactGroups = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups?pageNumber=1&pageSize=200");
            ViewBag.ContactGroups = contactGroups?.Data;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow creation data");
            TempData["Error"] = "Failed to load required data";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Save new workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkflow([FromBody] object workflowData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/workflows", workflowData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Edit workflow page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var workflow = await _apiClient.GetAsync<ApiResponse<object>>($"/api/workflows/{id}");
            var contactGroups = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups?pageNumber=1&pageSize=200");

            if (workflow?.Success == true && workflow.Data != null)
            {
                ViewBag.WorkflowId = id;
                ViewBag.ContactGroups = contactGroups?.Data;
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
    /// Update workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateWorkflow(int id, [FromBody] object workflowData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/workflows/{id}", workflowData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow");
            return Json(new { success = false, message = "An error occurred" });
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
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/workflows/{id}/activate", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow activated" });
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
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/workflows/{id}/pause", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow paused" });
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
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/workflows/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Duplicate workflow (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/workflows/{id}/duplicate", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Workflow duplicated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating workflow {WorkflowId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// View workflow details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var workflow = await _apiClient.GetAsync<ApiResponse<object>>($"/api/workflows/{id}");
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
}
