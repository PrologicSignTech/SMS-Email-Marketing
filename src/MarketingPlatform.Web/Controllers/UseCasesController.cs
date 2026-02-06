using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing landing page use cases
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class UseCasesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<UseCasesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UseCasesController(
        IApiClient apiClient,
        ILogger<UseCasesController> logger,
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
    /// Display use cases list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get use cases data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetUseCases([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/usecases");

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;

                if (dataObj.HasValue)
                {
                    var items = dataObj.Value;

                    return Json(new
                    {
                        draw = request.Draw,
                        recordsTotal = items.GetArrayLength(),
                        recordsFiltered = items.GetArrayLength(),
                        data = items
                    });
                }
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
            _logger.LogError(ex, "Error fetching use cases data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load use cases"
            });
        }
    }

    /// <summary>
    /// Delete use case (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/usecases/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting use case {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new use case page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load categories or industries if needed
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading use case creation data");
            TempData["Error"] = "Failed to load required data for use case creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit use case page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var useCase = await _apiClient.GetAsync<ApiResponse<object>>($"/api/usecases/{id}");

            if (useCase?.Success == true && useCase.Data != null)
            {
                ViewBag.UseCaseId = id;
                return View(useCase.Data);
            }

            TempData["Error"] = "Use case not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading use case {Id}", id);
            TempData["Error"] = "Failed to load use case";
            return RedirectToAction(nameof(Index));
        }
    }
}
