using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing testimonials
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class TestimonialsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TestimonialsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestimonialsController(
        IApiClient apiClient,
        ILogger<TestimonialsController> logger,
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
    /// Display testimonials list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get testimonials data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetTestimonials([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/testimonials");

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
            _logger.LogError(ex, "Error fetching testimonials data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load testimonials"
            });
        }
    }

    /// <summary>
    /// Delete testimonial (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/testimonials/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting testimonial {Id}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new testimonial page
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Edit testimonial page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var testimonial = await _apiClient.GetAsync<ApiResponse<object>>($"/api/testimonials/{id}");

            if (testimonial?.Success == true && testimonial.Data != null)
            {
                ViewBag.TestimonialId = id;
                return View(testimonial.Data);
            }

            TempData["Error"] = "Testimonial not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading testimonial {Id}", id);
            TempData["Error"] = "Failed to load testimonial";
            return RedirectToAction(nameof(Index));
        }
    }
}
