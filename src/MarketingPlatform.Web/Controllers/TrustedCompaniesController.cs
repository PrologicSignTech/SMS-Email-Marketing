using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing trusted companies
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class TrustedCompaniesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TrustedCompaniesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TrustedCompaniesController(
        IApiClient apiClient,
        ILogger<TrustedCompaniesController> logger,
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
    /// Display trusted companies list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get trusted companies data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetCompanies([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/trustedcompanies");

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
            _logger.LogError(ex, "Error fetching trusted companies data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load trusted companies"
            });
        }
    }

    /// <summary>
    /// Delete trusted company (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/trustedcompanies/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting trusted company {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new trusted company page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Edit trusted company page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var company = await _apiClient.GetAsync<ApiResponse<object>>($"/api/trustedcompanies/{id}");

            if (company?.Success == true && company.Data != null)
            {
                ViewBag.CompanyId = id;
                return View(company.Data);
            }

            TempData["Error"] = "Company not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trusted company {Id}", id);
            TempData["Error"] = "Failed to load company";
            return RedirectToAction(nameof(Index));
        }
    }
}
