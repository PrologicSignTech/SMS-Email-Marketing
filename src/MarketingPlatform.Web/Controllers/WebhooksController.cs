using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for webhook management
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class WebhooksController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebhooksController(
        IApiClient apiClient,
        ILogger<WebhooksController> logger,
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
    /// Display webhooks list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get webhooks data for DataTables (SERVER-SIDE AJAX)
    /// Web server calls API, not browser - more secure
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetWebhooks([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };

        try
        {
            // Webhook listing API not yet implemented — return empty for now
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
            _logger.LogError(ex, "Error fetching webhooks data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load webhooks"
            });
        }
    }

    /// <summary>
    /// Test webhook (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Test(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<string>>(
                $"/api/webhooks/{id}/test",
                new { }
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing webhook {WebhookId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete webhook (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>(
                $"/api/webhooks/{id}"
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook {WebhookId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new webhook
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load available event types for webhook subscription
            var eventTypes = await _apiClient.GetAsync<ApiResponse<object>>("/api/webhooks/eventtypes");
            ViewBag.EventTypes = eventTypes?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading webhook creation data");
            TempData["Error"] = "Failed to load required data for webhook creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit webhook
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var webhookTask = _apiClient.GetAsync<ApiResponse<object>>($"/api/webhooks/{id}");
            var eventTypesTask = _apiClient.GetAsync<ApiResponse<object>>("/api/webhooks/eventtypes");

            await Task.WhenAll(webhookTask, eventTypesTask);

            var webhook = await webhookTask;
            var eventTypes = await eventTypesTask;

            if (webhook?.Success == true && webhook.Data != null)
            {
                ViewBag.WebhookId = id;
                ViewBag.EventTypes = eventTypes?.Data;
                return View(webhook.Data);
            }

            TempData["Error"] = "Webhook not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading webhook {WebhookId}", id);
            TempData["Error"] = "Failed to load webhook";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Webhook logs and testing
    /// </summary>
    public IActionResult Logs(int id)
    {
        ViewBag.WebhookId = id;
        return View();
    }
}
