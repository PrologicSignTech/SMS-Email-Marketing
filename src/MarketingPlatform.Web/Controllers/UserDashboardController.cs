/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers
{
    /// <summary>
    /// User Dashboard Controller - For regular users to access platform features
    /// </summary>
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<UserDashboardController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserDashboardController(IApiClient apiClient, ILogger<UserDashboardController> logger, IHttpContextAccessor httpContextAccessor)
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

        /// <summary>
        /// User Dashboard Index page - loads data dynamically via AJAX
        /// </summary>
        public IActionResult Index()
        {
            // Pass the authenticated user's name to the view
            ViewBag.UserName = User.Identity?.Name ?? "User";
            return View();
        }

        /// <summary>
        /// Get dashboard stats: total campaigns, active campaigns, total contacts, messages sent, engagement rate
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // Call the analytics dashboard API to get overall stats
                var analyticsResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/dashboard");

                if (analyticsResult?.Success == true && analyticsResult.Data != null)
                {
                    return Json(new { success = true, data = analyticsResult.Data });
                }

                // If analytics endpoint doesn't return data, gather stats from individual endpoints
                var stats = new Dictionary<string, object>();

                // Get campaigns count
                try
                {
                    var campaignsResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns?pageNumber=1&pageSize=1");
                    if (campaignsResult?.Success == true && campaignsResult.Data != null)
                    {
                        var dataObj = campaignsResult.Data as System.Text.Json.JsonElement?;
                        if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (dataObj.Value.TryGetProperty("totalCount", out var totalCount))
                                stats["totalCampaigns"] = totalCount.GetInt32();
                        }
                    }
                }
                catch { /* continue with defaults */ }

                // Get active campaigns count
                try
                {
                    var activeResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns/status/Active");
                    if (activeResult?.Success == true && activeResult.Data != null)
                    {
                        var dataObj = activeResult.Data as System.Text.Json.JsonElement?;
                        if (dataObj.HasValue)
                        {
                            if (dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                                stats["activeCampaigns"] = dataObj.Value.GetArrayLength();
                            else if (dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object && dataObj.Value.TryGetProperty("totalCount", out var ac))
                                stats["activeCampaigns"] = ac.GetInt32();
                        }
                    }
                }
                catch { /* continue with defaults */ }

                // Get contacts count
                try
                {
                    var contactsResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/contacts?pageNumber=1&pageSize=1");
                    if (contactsResult?.Success == true && contactsResult.Data != null)
                    {
                        var dataObj = contactsResult.Data as System.Text.Json.JsonElement?;
                        if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (dataObj.Value.TryGetProperty("totalCount", out var totalContacts))
                                stats["totalContacts"] = totalContacts.GetInt32();
                        }
                    }
                }
                catch { /* continue with defaults */ }

                // Get messages count
                try
                {
                    var messagesResult = await _apiClient.GetAsync<ApiResponse<object>>("/api/messages?pageNumber=1&pageSize=1");
                    if (messagesResult?.Success == true && messagesResult.Data != null)
                    {
                        var dataObj = messagesResult.Data as System.Text.Json.JsonElement?;
                        if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (dataObj.Value.TryGetProperty("totalCount", out var totalMessages))
                                stats["messagesSent"] = totalMessages.GetInt32();
                        }
                    }
                }
                catch { /* continue with defaults */ }

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard stats");
                return Json(new { success = false, message = "An error occurred while loading dashboard stats" });
            }
        }

        /// <summary>
        /// Get recent campaigns for the user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyCampaigns()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/campaigns?pageNumber=1&pageSize=5");
                if (result?.Success == true && result.Data != null)
                {
                    // Extract items from paginated result
                    var dataObj = result.Data as System.Text.Json.JsonElement?;
                    if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                        && dataObj.Value.TryGetProperty("items", out var itemsElement))
                    {
                        return Json(new { success = true, data = itemsElement });
                    }
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = true, data = new object[] { } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user campaigns");
                return Json(new { success = false, message = "An error occurred while loading campaigns" });
            }
        }

        /// <summary>
        /// Get campaign performance data for top campaigns chart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopCampaigns()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/analytics/campaigns/performance");
                if (result?.Success == true && result.Data != null)
                {
                    // Extract items if paginated
                    var dataObj = result.Data as System.Text.Json.JsonElement?;
                    if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                        && dataObj.Value.TryGetProperty("items", out var itemsElement))
                    {
                        return Json(new { success = true, data = itemsElement });
                    }
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = true, data = new object[] { } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top campaigns");
                return Json(new { success = false, message = "An error occurred while loading top campaigns" });
            }
        }

        /// <summary>
        /// Get recent activities / audit trail
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                // Try analytics dashboard for recent activity data
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/messages?pageNumber=1&pageSize=10");
                if (result?.Success == true && result.Data != null)
                {
                    // Extract items from paginated result
                    var dataObj = result.Data as System.Text.Json.JsonElement?;
                    if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                        && dataObj.Value.TryGetProperty("items", out var itemsElement))
                    {
                        return Json(new { success = true, data = itemsElement });
                    }
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = true, data = new object[] { } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent activities");
                return Json(new { success = false, message = "An error occurred while loading activities" });
            }
        }
    }
}
