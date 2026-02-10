using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

[Authorize]
public class SuppressionRulesController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SuppressionRulesController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuppressionRulesController(
        IApiClient apiClient,
        ILogger<SuppressionRulesController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
            _apiClient.SetAuthorizationToken(token);
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> GetRules([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 50 };
        var pageSize = request.Length > 0 ? request.Length : 50;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/suppressionrules?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=priority&sortDescending=false";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            var result = await _apiClient.GetAsync<ApiResponse<object>>(queryString);

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                var items = dataObj?.GetProperty("items");
                var totalCount = dataObj?.GetProperty("totalCount").GetInt32() ?? 0;

                return Json(new { draw = request.Draw, recordsTotal = totalCount, recordsFiltered = totalCount, data = items });
            }

            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching suppression rules");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load rules" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] object ruleData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/suppressionrules", ruleData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Rule created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating suppression rule");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] object ruleData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/suppressionrules/{id}", ruleData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Rule updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating suppression rule");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ToggleRule(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/suppressionrules/{id}/toggle", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Rule toggled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling suppression rule");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRule(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/suppressionrules/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting suppression rule");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SeedDefaults()
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/suppressionrules/seed-defaults", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Default rules seeded" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default rules");
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
