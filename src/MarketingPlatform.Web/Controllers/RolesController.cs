using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for role and permission management
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class RolesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<RolesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RolesController(
        IApiClient apiClient,
        ILogger<RolesController> logger,
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
    /// Display roles list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get roles data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetRoles([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/roles",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "name",
                    sortDirection = "asc"
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
            _logger.LogError(ex, "Error fetching roles data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load roles"
            });
        }
    }

    /// <summary>
    /// Delete role (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/roles/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new role
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load available permissions for role assignment
            var permissions = await _apiClient.GetAsync<ApiResponse<object>>("/api/permissions");
            ViewBag.Permissions = permissions?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role creation data");
            TempData["Error"] = "Failed to load required data for role creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit role
    /// </summary>
    public async Task<IActionResult> Edit(string id)
    {
        try
        {
            var roleTask = _apiClient.GetAsync<ApiResponse<object>>($"/api/roles/{id}");
            var permissionsTask = _apiClient.GetAsync<ApiResponse<object>>("/api/permissions");

            await Task.WhenAll(roleTask, permissionsTask);

            var role = await roleTask;
            var permissions = await permissionsTask;

            if (role?.Success == true && role.Data != null)
            {
                ViewBag.RoleId = id;
                ViewBag.Permissions = permissions?.Data;
                return View(role.Data);
            }

            TempData["Error"] = "Role not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role {RoleId}", id);
            TempData["Error"] = "Failed to load role";
            return RedirectToAction(nameof(Index));
        }
    }
}
