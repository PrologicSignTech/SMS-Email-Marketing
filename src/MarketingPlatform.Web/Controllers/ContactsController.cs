using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for managing contacts and contact groups
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class ContactsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ContactsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContactsController(
        IApiClient apiClient,
        ILogger<ContactsController> logger,
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
    /// Display contacts list
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get contacts data for DataTables (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetContacts([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/contacts",
                new
                {
                    pageNumber = (request.Start / request.Length) + 1,
                    pageSize = request.Length,
                    searchTerm = request.Search?.Value,
                    sortColumn = "createdAt",
                    sortDirection = "desc",
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
            _logger.LogError(ex, "Error fetching contacts data");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new object[] { },
                error = "Failed to load contacts"
            });
        }
    }

    /// <summary>
    /// Get contact groups (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactGroups()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");

            if (result?.Success == true)
            {
                return Json(new { success = true, items = result.Data });
            }

            return Json(new { success = false, items = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contact groups");
            return Json(new { success = false, items = new object[] { } });
        }
    }

    /// <summary>
    /// Delete contact (SERVER-SIDE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/contacts/{id}");

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new contact
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Load contact groups for dropdown
            var contactGroups = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");
            ViewBag.ContactGroups = contactGroups?.Data;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact creation data");
            TempData["Error"] = "Failed to load required data for contact creation";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// View contact details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var contact = await _apiClient.GetAsync<ApiResponse<object>>($"/api/contacts/{id}");

            if (contact?.Success == true && contact.Data != null)
            {
                return View(contact.Data);
            }

            TempData["Error"] = "Contact not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact {ContactId}", id);
            TempData["Error"] = "Failed to load contact";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Manage contact groups
    /// </summary>
    public IActionResult Groups()
    {
        return View();
    }
}
