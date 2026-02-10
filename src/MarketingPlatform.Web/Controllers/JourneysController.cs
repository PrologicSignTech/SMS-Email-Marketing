using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing customer journeys and automation
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class JourneysController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<JourneysController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JourneysController(
        IApiClient apiClient,
        ILogger<JourneysController> logger,
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
    /// Display journeys list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get journeys data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetJourneys([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/journeys?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
            _logger.LogError(ex, "Error fetching journeys data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load journeys"
            });
        }
    }

    /// <summary>
    /// Activate journey (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/journeys/{id}/activate",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating journey {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Pause journey (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Pause(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/journeys/{id}/pause",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing journey {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete journey (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/journeys/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journey {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new journey page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load reference data needed for journey creation
            var contactGroupsTask = _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");
            var templatesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/templates");

            await Task.WhenAll(contactGroupsTask, templatesTask);

            var contactGroups = await contactGroupsTask;
            var templates = await templatesTask;

            ViewBag.ContactGroups = contactGroups?.Data;
            ViewBag.Templates = templates?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading journey creation data");
            TempData["Error"] = "Failed to load required data for journey creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit journey page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var journey = await _apiClient.GetAsync<ApiResponse<object>>($"/api/journeys/{id}");

            if (journey?.Success == true && journey.Data != null)
            {
                ViewBag.JourneyId = id;
                return View(journey.Data);
            }

            TempData["Error"] = "Journey not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading journey {JourneyId}", id);
            TempData["Error"] = "Failed to load journey";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// View journey details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var journey = await _apiClient.GetAsync<ApiResponse<object>>($"/api/journeys/{id}");

            if (journey?.Success == true && journey.Data != null)
            {
                return View(journey.Data);
            }

            TempData["Error"] = "Journey not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading journey {JourneyId}", id);
            TempData["Error"] = "Failed to load journey";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Create journey (SERVER-SIDE POST)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object journeyData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/journeys", journeyData);

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = "Journey created successfully" });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to create journey" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journey");
            return Json(new { success = false, message = "An error occurred while creating the journey" });
        }
    }

    /// <summary>
    /// Update journey (SERVER-SIDE POST)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] object journeyData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>("/api/journeys", journeyData);

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = "Journey updated successfully" });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to update journey" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating journey");
            return Json(new { success = false, message = "An error occurred while updating the journey" });
        }
    }

    /// <summary>
    /// Get single journey (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetJourney(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/journeys/{id}");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Journey not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching journey {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred while fetching the journey" });
        }
    }

    /// <summary>
    /// Get journey statistics (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetJourneyStats(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/journeys/{id}/stats");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Stats not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching journey stats {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get journey executions (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetJourneyExecutions(int id, [FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var queryString = $"/api/journeys/{id}/executions?pageNumber={pageNumber}&pageSize={pageSize}";

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
            _logger.LogError(ex, "Error fetching journey executions {JourneyId}", id);
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { }
            });
        }
    }

    /// <summary>
    /// Duplicate journey (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                $"/api/journeys/{id}/duplicate",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed", data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating journey {JourneyId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Pause an execution (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PauseExecution(int executionId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                $"/api/journeys/executions/{executionId}/pause", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing execution {ExecutionId}", executionId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Resume an execution (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResumeExecution(int executionId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                $"/api/journeys/executions/{executionId}/resume", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming execution {ExecutionId}", executionId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Cancel an execution (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CancelExecution(int executionId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                $"/api/journeys/executions/{executionId}/cancel", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
