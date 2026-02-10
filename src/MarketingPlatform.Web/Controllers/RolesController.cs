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
    public async Task<IActionResult> GetRoles([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };

        try
        {
            // API returns a flat list (not paginated) via [HttpGet]
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/roles");

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                // API returns array directly, not PaginatedResult
                var items = dataObj;
                var totalCount = dataObj?.GetArrayLength() ?? 0;

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
    /// Create role page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Edit role page
    /// </summary>
    public IActionResult Edit(int id)
    {
        ViewBag.RoleId = id;
        return View();
    }

    /// <summary>
    /// Get role details (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRole(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/roles/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create role (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] object roleData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/roles", roleData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Role created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update role (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] object roleData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/roles/{id}", roleData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Role updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
