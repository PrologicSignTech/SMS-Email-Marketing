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
    public async Task<IActionResult> GetMessages([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/messages?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=sentAt&sortDescending=true";
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
    /// Send/schedule a message (bulk send to group contacts)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] object messageData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                "/api/messages/bulk", messageData
            );

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Message sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return Json(new { success = false, message = "An error occurred while sending the message" });
        }
    }

    /// <summary>
    /// Get contact groups for recipient selection
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecipientGroups()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups?pageNumber=1&pageSize=100");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recipient groups");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get templates for template loading
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplatesForCompose(int? channel)
    {
        try
        {
            var endpoint = channel.HasValue
                ? $"/api/templates/channel/{channel.Value}"
                : "/api/templates?pageNumber=1&pageSize=100";
            var result = await _apiClient.GetAsync<ApiResponse<object>>(endpoint);
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching templates");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get user's assigned phone numbers for From Number dropdown
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyPhoneNumbers()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/phonenumbers/my");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user phone numbers");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get contacts for a specific group (for composing messages)
    /// Routes through Web controller to avoid direct browser → API calls
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGroupContacts(int groupId, int pageSize = 10000)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>(
                $"/api/contactgroups/{groupId}/contacts?pageNumber=1&pageSize={pageSize}"
            );
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contacts for group {GroupId}", groupId);
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get message details (SERVER-SIDE)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessage(int id)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/messages/{id}");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching message {MessageId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
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

    // ========================================================================
    // TWO-WAY MESSAGING INBOX
    // ========================================================================

    /// <summary>
    /// Two-way messaging inbox
    /// </summary>
    public IActionResult Inbox()
    {
        return View();
    }

    /// <summary>
    /// Get conversations for inbox
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/messages/conversations?pageNumber=1&pageSize=50");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get messages for a specific conversation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversationMessages(int conversationId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/messages/conversations/{conversationId}/messages");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversation messages for {ConversationId}", conversationId);
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Send a reply in a conversation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendReply([FromBody] object replyData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/messages/reply", replyData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Reply sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reply");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Assign a conversation to an agent
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignConversation([FromBody] object assignData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/messages/conversations/assign", assignData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Assigned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning conversation");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Mark a conversation as resolved
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResolveConversation([FromBody] object resolveData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/messages/conversations/resolve", resolveData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Resolved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving conversation");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Retry a failed message
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RetryMessage([FromBody] object data)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                System.Text.Json.JsonSerializer.Serialize(data));
            var messageId = json.GetProperty("messageId").GetInt32();
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/messages/{messageId}/retry", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Queued for retry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying message");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Cancel a scheduled message
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CancelMessage([FromBody] object data)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                System.Text.Json.JsonSerializer.Serialize(data));
            var messageId = json.GetProperty("messageId").GetInt32();
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/messages/{messageId}/cancel", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Message cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling message");
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
