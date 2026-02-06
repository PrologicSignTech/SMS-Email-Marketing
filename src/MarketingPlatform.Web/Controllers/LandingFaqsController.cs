using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing landing page FAQs
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class LandingFaqsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LandingFaqsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LandingFaqsController(
        IApiClient apiClient,
        ILogger<LandingFaqsController> logger,
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
    /// Display landing FAQs list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get landing FAQs data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetFaqs([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/landingfaqs");

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
            _logger.LogError(ex, "Error fetching landing FAQs data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load landing FAQs"
            });
        }
    }

    /// <summary>
    /// Delete landing FAQ (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/landingfaqs/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting landing FAQ {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new landing FAQ page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load icon options or categories if needed
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ creation data");
            TempData["Error"] = "Failed to load required data for FAQ creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit landing FAQ page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var faq = await _apiClient.GetAsync<ApiResponse<object>>($"/api/landingfaqs/{id}");

            if (faq?.Success == true && faq.Data != null)
            {
                ViewBag.FaqId = id;
                return View(faq.Data);
            }

            TempData["Error"] = "FAQ not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading landing FAQ {Id}", id);
            TempData["Error"] = "Failed to load FAQ";
            return RedirectToAction(nameof(Index));
        }
    }
}
