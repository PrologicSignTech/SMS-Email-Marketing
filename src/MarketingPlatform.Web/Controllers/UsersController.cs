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
    public async Task<IActionResult> GetUsers([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/users?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
    /// Get user details (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/users/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Activate user (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Activate(string id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/users/{id}/activate", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "User activated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Deactivate user (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Deactivate(string id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/users/{id}/deactivate", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "User deactivated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update user profile (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] object userData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/users/profile", userData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "User updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
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

    /// <summary>
    /// Get list of agents (users) for conversation assignment
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/users?pageNumber=1&pageSize=50&role=User");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching agents");
            return Json(new { success = false, data = new object[] { } });
        }
    }
}
