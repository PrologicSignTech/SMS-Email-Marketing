using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for analytics and reporting
/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
/// </summary>
[Authorize]
public class AnalyticsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<AnalyticsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AnalyticsController(
        IApiClient apiClient,
        ILogger<AnalyticsController> logger,
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
    /// Display analytics dashboard
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get dashboard analytics data (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/dashboard");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load analytics data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics dashboard data");
            return Json(new { success = false, message = "An error occurred while loading analytics" });
        }
    }

    /// <summary>
    /// Campaign performance report
    /// </summary>
    public IActionResult Campaigns()
    {
        return View();
    }

    /// <summary>
    /// Get campaign analytics data (SERVER-SIDE AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignAnalytics()
    {
        try
        {
            // Use the correct API endpoint
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/campaigns/performance");

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to load campaign analytics" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign analytics data");
            return Json(new { success = false, message = "An error occurred while loading campaign analytics" });
        }
    }

    /// <summary>
    /// Get specific campaign performance (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignPerformance(int campaignId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/analytics/campaigns/{campaignId}/performance");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = result?.Message ?? "No performance data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign {CampaignId} performance", campaignId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    // ========================================================================
    // CONVERSION TRACKING
    // ========================================================================

    /// <summary>
    /// Conversions tracking page
    /// </summary>
    public IActionResult Conversions()
    {
        return View();
    }

    /// <summary>
    /// Get all conversions data (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversions(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/conversions?";
            if (!string.IsNullOrEmpty(startDate))
                queryString += $"startDate={Uri.EscapeDataString(startDate)}&";
            if (!string.IsNullOrEmpty(endDate))
                queryString += $"endDate={Uri.EscapeDataString(endDate)}&";

            var result = await _apiClient.GetAsync<ApiResponse<object>>(queryString.TrimEnd('&', '?'));
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversions data");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get campaign conversions (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignConversions(int campaignId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/analytics/campaigns/{campaignId}/conversions");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = "No conversion data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversions for campaign {CampaignId}", campaignId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    // ========================================================================
    // EXPORT ENDPOINTS
    // ========================================================================

    /// <summary>
    /// Export campaign performance to CSV (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportCampaignPerformanceCsv(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/campaigns/performance/export/csv?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "campaign-performance.csv", "text/csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting campaign performance CSV");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Export campaign performance to Excel (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportCampaignPerformanceExcel(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/campaigns/performance/export/excel?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "campaign-performance.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting campaign performance Excel");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Export contact engagement to CSV (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportEngagementCsv(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/contacts/engagement/export/csv?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "contact-engagement.csv", "text/csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting engagement CSV");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Export contact engagement to Excel (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportEngagementExcel(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/contacts/engagement/export/excel?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "contact-engagement.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting engagement Excel");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Export conversions to CSV (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportConversionsCsv(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/conversions/export/csv?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "conversions.csv", "text/csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting conversions CSV");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Export conversions to Excel (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportConversionsExcel(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/conversions/export/excel?";
            if (!string.IsNullOrEmpty(startDate)) queryString += $"startDate={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) queryString += $"endDate={endDate}&";

            return await ProxyFileDownload(queryString.TrimEnd('&', '?'), "conversions.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting conversions Excel");
            return Json(new { success = false, message = "Export failed" });
        }
    }

    /// <summary>
    /// Helper: Proxy a file download from the API
    /// </summary>
    private async Task<IActionResult> ProxyFileDownload(string apiEndpoint, string fileName, string contentType)
    {
        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        using var httpClient = new HttpClient();
        var baseUrl = _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"] ?? "https://localhost:7011";
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(60);
        if (!string.IsNullOrEmpty(token))
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync(apiEndpoint);

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            return File(fileBytes, contentType, fileName);
        }

        return Json(new { success = false, message = "Export failed" });
    }

    /// <summary>
    /// Message reports page
    /// </summary>
    public IActionResult Reports()
    {
        return View();
    }

    /// <summary>
    /// Get messages for reports DataTable (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetReportMessages([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";

            // If filtering by status, use status-specific endpoint
            if (request.Status.HasValue)
            {
                try
                {
                    var statusResult = await _apiClient.GetAsync<ApiResponse<object>>($"/api/messages/status/{request.Status.Value}");
                    if (statusResult?.Success == true)
                    {
                        var statusData = statusResult.Data as System.Text.Json.JsonElement?;
                        var allItems = new List<System.Text.Json.JsonElement>();

                        if (statusData?.ValueKind == System.Text.Json.JsonValueKind.Array)
                            allItems = statusData.Value.EnumerateArray().ToList();
                        else if (statusData?.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            try { allItems = statusData.Value.GetProperty("items").EnumerateArray().ToList(); }
                            catch { }
                        }

                        // Client-side search
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            allItems = allItems.Where(item =>
                            {
                                try
                                {
                                    var recipient = item.GetProperty("recipient").GetString() ?? "";
                                    return recipient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                                }
                                catch { return false; }
                            }).ToList();
                        }

                        var totalFiltered = allItems.Count;
                        var pagedItems = allItems.Skip(request.Start).Take(pageSize).ToList();

                        return Json(new
                        {
                            draw = request.Draw,
                            recordsTotal = totalFiltered,
                            recordsFiltered = totalFiltered,
                            data = pagedItems
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Status filter failed, falling back to default");
                }
            }

            var queryString = $"/api/messages?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
            _logger.LogError(ex, "Error fetching report messages");
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
}
