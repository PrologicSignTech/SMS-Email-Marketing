using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for user management and profile
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class UsersController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<UsersController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersController(
        IApiClient apiClient,
        ILogger<UsersController> logger,
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
    /// Display users list (Admin only)
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get users data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetUsers([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/users",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "createdAt",
                    sortDirection = "desc",
                    roleId = request.RoleId,
                    status = request.Status
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
            _logger.LogError(ex, "Error fetching users data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load users"
            });
        }
    }

    /// <summary>
    /// Delete user (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/users/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// User dashboard
    /// </summary>
    public IActionResult Dashboard()
    {
        _logger.LogInformation("Dashboard accessed. User authenticated: {IsAuthenticated}, User: {User}",
            User.Identity?.IsAuthenticated,
            User.Identity?.Name ?? "Anonymous");

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("User not authenticated when accessing Dashboard - will be redirected to login");
        }
        else
        {
            _logger.LogInformation("User {User} successfully authenticated on Dashboard", User.Identity.Name);
        }

        return View();
    }

    /// <summary>
    /// View/Edit user profile
    /// </summary>
    public IActionResult Profile()
    {
        return View();
    }

    /// <summary>
    /// User settings
    /// </summary>
    public IActionResult Settings()
    {
        return View();
    }
}
