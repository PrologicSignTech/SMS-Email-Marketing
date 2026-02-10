using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Web.Models;

namespace MarketingPlatform.Web.Controllers;

[Authorize]
public class PhoneNumbersController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<PhoneNumbersController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PhoneNumbersController(
        IApiClient apiClient,
        ILogger<PhoneNumbersController> logger,
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

    public IActionResult Buy() => View();

    public IActionResult Assign()
    {
        // Only SuperAdmin and Admin can access Assign page
        var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            return Forbid();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetPhoneNumbers([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/phonenumbers?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
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
            _logger.LogError(ex, "Error fetching phone numbers data");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load phone numbers" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNumbers()
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

    [HttpGet]
    public async Task<IActionResult> GetAvailableNumbers()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/phonenumbers/available");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available phone numbers");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddNumber([FromBody] object numberData)
    {
        // Only SuperAdmin and Admin can add numbers
        var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            return Json(new { success = false, message = "Unauthorized" });

        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/phonenumbers", numberData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number added" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> PurchaseNumber([FromBody] object purchaseData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/phonenumbers/purchase", purchaseData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number purchased" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AssignNumber(int id, [FromBody] object assignData)
    {
        // Only SuperAdmin and Admin can assign numbers
        var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            return Json(new { success = false, message = "Unauthorized" });

        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/phonenumbers/{id}/assign", assignData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number assigned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UnassignNumber(int id)
    {
        // Only SuperAdmin and Admin can unassign numbers
        var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            return Json(new { success = false, message = "Unauthorized" });

        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/phonenumbers/{id}/unassign", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number unassigned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNumber(int id, [FromBody] object updateData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/phonenumbers/{id}", updateData);
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReleaseNumber(int id)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/phonenumbers/{id}/release", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Number released" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/phonenumbers/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting phone number");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/users?pageNumber=1&pageSize=100");
            return Json(new { success = result?.Success ?? false, data = result?.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Search available (unassigned) numbers in our database
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchAvailableNumbers(string? country, int? numberType, int? capabilities, string? contains)
    {
        try
        {
            var qs = "/api/phonenumbers/available?";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(country)) queryParams.Add($"country={Uri.EscapeDataString(country)}");
            if (numberType.HasValue) queryParams.Add($"numberType={numberType.Value}");
            if (capabilities.HasValue) queryParams.Add($"capabilities={capabilities.Value}");
            if (!string.IsNullOrEmpty(contains)) queryParams.Add($"contains={Uri.EscapeDataString(contains)}");
            qs += string.Join("&", queryParams);

            var result = await _apiClient.GetAsync<ApiResponse<object>>(qs);
            if (result?.Success == true)
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
            return Json(new { success = false, data = new object[] { }, message = result?.Message ?? "No numbers found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching available phone numbers");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Search provider numbers (Twilio/Nexmo/Plivo) - SuperAdmin only
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchProviderNumbers(string? country, int? numberType, int? capabilities, string? contains)
    {
        try
        {
            // Verify SuperAdmin role
            var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roles.Contains("SuperAdmin"))
            {
                return Json(new { success = false, data = new object[] { }, message = "Unauthorized" });
            }

            var qs = "/api/phonenumbers/search-provider?";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(country)) queryParams.Add($"country={Uri.EscapeDataString(country)}");
            if (numberType.HasValue) queryParams.Add($"numberType={numberType.Value}");
            if (capabilities.HasValue) queryParams.Add($"capabilities={capabilities.Value}");
            if (!string.IsNullOrEmpty(contains)) queryParams.Add($"contains={Uri.EscapeDataString(contains)}");
            qs += string.Join("&", queryParams);

            var result = await _apiClient.GetAsync<ApiResponse<object>>(qs);
            if (result?.Success == true)
            {
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && dataObj.Value.TryGetProperty("items", out var itemsElement))
                {
                    return Json(new { success = true, data = itemsElement });
                }
                return Json(new { success = true, data = result.Data });
            }
            return Json(new { success = false, data = new object[] { }, message = result?.Message ?? "No provider numbers found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching provider phone numbers");
            return Json(new { success = false, data = new object[] { } });
        }
    }
}
