using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Compliance & Consent Management Web Controller
/// SERVER-SIDE API INTEGRATION
/// </summary>
[Authorize]
public class ComplianceController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ComplianceController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ComplianceController(
        IApiClient apiClient,
        ILogger<ComplianceController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _apiClient.SetAuthorizationToken(token);
        }
    }

    // ========================================================================
    // CONSENT AUDIT LOG
    // ========================================================================

    /// <summary>
    /// Consent Audit Log page
    /// </summary>
    public IActionResult AuditLog()
    {
        return View();
    }

    /// <summary>
    /// Get audit logs (SERVER-SIDE AJAX for DataTable)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetAuditLogs([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";

            var queryString = $"/api/compliance/audit-logs?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=actionDate&sortDescending=true";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            var result = await _apiClient.GetAsync<ApiResponse<object>>(queryString);

            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                object? items = null;
                int totalCount = 0;

                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (dataObj.Value.TryGetProperty("items", out var itemsEl))
                        items = itemsEl;
                    if (dataObj.Value.TryGetProperty("totalCount", out var totalEl))
                        totalCount = totalEl.GetInt32();
                }
                else if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    items = dataObj.Value;
                    totalCount = dataObj.Value.GetArrayLength();
                }

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = items ?? new object[] { }
                });
            }

            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching compliance audit logs");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load audit logs" });
        }
    }

    // ========================================================================
    // COMPLIANCE SETTINGS
    // ========================================================================

    /// <summary>
    /// Compliance settings page
    /// </summary>
    public IActionResult Settings()
    {
        return View();
    }

    /// <summary>
    /// Get compliance settings (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/compliance/settings");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = result?.Message ?? "Failed to load settings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching compliance settings");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update compliance settings (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] object settingsData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>("/api/compliance/settings", settingsData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Settings updated" });
            return Json(new { success = false, message = result?.Message ?? "Failed to update settings" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error updating compliance settings");
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating compliance settings");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ========================================================================
    // CONSENT MANAGEMENT
    // ========================================================================

    /// <summary>
    /// Get consent status for a contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConsentStatus(int contactId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/compliance/contacts/{contactId}/consent-status");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = result?.Message ?? "No consent data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching consent status for contact {ContactId}", contactId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get consent history for a contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConsentHistory(int contactId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/compliance/contacts/{contactId}/consent-history?pageNumber=1&pageSize=50");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching consent history for contact {ContactId}", contactId);
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Record consent (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RecordConsent([FromBody] object consentData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/compliance/consent", consentData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Consent recorded" });
            return Json(new { success = false, message = result?.Message ?? "Failed to record consent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording consent");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Revoke consent (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RevokeConsent(int contactId, int channel, string? reason)
    {
        try
        {
            var url = $"/api/compliance/contacts/{contactId}/revoke-consent?channel={channel}";
            if (!string.IsNullOrEmpty(reason))
                url += $"&reason={Uri.EscapeDataString(reason)}";

            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(url, new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent for contact {ContactId}", contactId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Check quiet hours (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckQuietHours()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/compliance/quiet-hours/check");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking quiet hours");
            return Json(new { success = false });
        }
    }
}
