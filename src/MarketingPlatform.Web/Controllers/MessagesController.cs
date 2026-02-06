using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for message management and composition
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class MessagesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MessagesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MessagesController(
        IApiClient apiClient,
        ILogger<MessagesController> logger,
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
    /// Display messages list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get messages data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetMessages([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/messages",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "sentAt",
                    sortDirection = "desc",
                    status = request.Status,
                    channel = request.Channel
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
            _logger.LogError(ex, "Error fetching messages data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load messages"
            });
        }
    }

    /// <summary>
    /// Duplicate message (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/messages/{id}/duplicate",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating message {MessageId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Compose new message
    /// </summary>
    public IActionResult Compose()
    {
        return View();
    }

    /// <summary>
    /// View message details
    /// </summary>
    public IActionResult Details(int id)
    {
        ViewBag.MessageId = id;
        return View();
    }

    /// <summary>
    /// Message preview and testing
    /// </summary>
    public IActionResult Preview()
    {
        return View();
    }
}
