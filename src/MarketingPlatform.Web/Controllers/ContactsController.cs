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
    public async Task<IActionResult> GetContacts([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";
            var queryString = $"/api/contacts?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=createdAt&sortDescending=true";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (request.GroupId.HasValue)
                queryString += $"&groupId={request.GroupId.Value}";

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
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups?pageNumber=1&pageSize=200");

            if (result?.Success == true)
            {
                // result.Data is a PaginatedResult { items: [...], totalCount, ... }
                // Extract the items array for the frontend
                var dataObj = result.Data as System.Text.Json.JsonElement?;
                if (dataObj.HasValue && dataObj.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && dataObj.Value.TryGetProperty("items", out var itemsElement))
                {
                    return Json(new { success = true, items = itemsElement });
                }
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
    /// Edit contact page
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var contact = await _apiClient.GetAsync<ApiResponse<object>>($"/api/contacts/{id}");
            var contactGroups = await _apiClient.GetAsync<ApiResponse<object>>("/api/contactgroups");

            if (contact?.Success == true && contact.Data != null)
            {
                ViewBag.ContactGroups = contactGroups?.Data;
                ViewBag.ContactId = id;
                ViewBag.ContactData = contact.Data;
                return View(contact.Data);
            }

            TempData["Error"] = "Contact not found";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact {ContactId} for edit", id);
            TempData["Error"] = "Failed to load contact";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Create contact via API (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] object contactData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/contacts", contactData);

            if (result?.Success == true)
            {
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Contact created" });
            }

            return Json(new { success = false, message = result?.Message ?? "Failed to create contact" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error creating contact");
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Add contact to a group (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddContactToGroup(int groupId, int contactId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
                $"/api/contactgroups/{groupId}/contacts/{contactId}", new { });

            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contact {ContactId} to group {GroupId}", contactId, groupId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Manage contact groups
    /// </summary>
    public IActionResult Groups()
    {
        return View();
    }

    // ========================================================================
    // TAG MANAGEMENT
    // ========================================================================

    /// <summary>
    /// Tag management page
    /// </summary>
    public IActionResult Tags()
    {
        return View();
    }

    /// <summary>
    /// Get paginated tags (SERVER-SIDE AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetTags([FromBody] DataTablesRequest? request)
    {
        request ??= new DataTablesRequest { Draw = 1, Start = 0, Length = 25 };
        var pageSize = request.Length > 0 ? request.Length : 25;

        try
        {
            var pageNumber = (request.Start / pageSize) + 1;
            var searchTerm = request.Search?.Value ?? "";

            var queryString = $"/api/contacttags?pageNumber={pageNumber}&pageSize={pageSize}&sortBy=name&sortDescending=false";
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

            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tags");
            return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[] { }, error = "Failed to load tags" });
        }
    }

    /// <summary>
    /// Create tag (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] object tagData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/contacttags", tagData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Tag created" });
            return Json(new { success = false, message = result?.Message ?? "Failed to create tag" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error creating tag");
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update tag (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] object tagData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/contacttags/{id}", tagData);
            if (result?.Success == true)
                return Json(new { success = true, message = result.Message ?? "Tag updated" });
            return Json(new { success = false, message = result?.Message ?? "Failed to update tag" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error updating tag {TagId}", id);
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete tag (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/contacttags/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get tags for a contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactTags(int contactId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/contacttags/contacts/{contactId}");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tags for contact {ContactId}", contactId);
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Assign tag to contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignTag(int contactId, int tagId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/contacttags/contacts/{contactId}/tags/{tagId}", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning tag {TagId} to contact {ContactId}", tagId, contactId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Remove tag from contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RemoveTag(int contactId, int tagId)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/contacttags/contacts/{contactId}/tags/{tagId}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag {TagId} from contact {ContactId}", tagId, contactId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    // ========================================================================
    // DUPLICATE DETECTION & MERGE
    // ========================================================================

    /// <summary>
    /// Duplicate detection & merge page
    /// </summary>
    public IActionResult Duplicates()
    {
        return View();
    }

    /// <summary>
    /// Get duplicates report (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDuplicatesReport()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contacts/duplicates/report");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = result?.Message ?? "Failed to load duplicates report" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching duplicates report");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Resolve duplicates (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResolveDuplicates([FromBody] object resolveData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/contacts/duplicates/resolve", resolveData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Duplicates resolved" });
            return Json(new { success = false, message = result?.Message ?? "Failed to resolve duplicates" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error resolving duplicates");
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving duplicates");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ========================================================================
    // CSV / EXCEL IMPORT & EXPORT
    // ========================================================================

    /// <summary>
    /// Import contacts page
    /// </summary>
    public IActionResult Import()
    {
        return View();
    }

    /// <summary>
    /// Import CSV file (SERVER-SIDE proxy with file upload)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ImportCsv(IFormFile file, int? groupId)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a file to import" });

            // Forward the file to the API as multipart/form-data
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "text/csv");
            content.Add(fileContent, "file", file.FileName);

            var endpoint = "/api/contacts/import/csv";
            if (groupId.HasValue)
                endpoint += $"?groupId={groupId.Value}";

            // Use HttpClient directly for multipart upload
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
            using var httpClient = new HttpClient();
            var baseUrl = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"] ?? "https://localhost:7011";
            httpClient.BaseAddress = new Uri(baseUrl);
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(responseBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (apiResponse?.Success == true)
                    return Json(new { success = true, data = apiResponse.Data, message = apiResponse.Message ?? "Import completed" });
                return Json(new { success = false, message = apiResponse?.Message ?? "Import failed" });
            }

            return Json(new { success = false, message = "Import failed. Please check your file format." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing CSV");
            return Json(new { success = false, message = "An error occurred during import" });
        }
    }

    /// <summary>
    /// Import Excel file (SERVER-SIDE proxy with file upload)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile file, int? groupId)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a file to import" });

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            content.Add(fileContent, "file", file.FileName);

            var endpoint = "/api/contacts/import/excel";
            if (groupId.HasValue)
                endpoint += $"?groupId={groupId.Value}";

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
            using var httpClient = new HttpClient();
            var baseUrl = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"] ?? "https://localhost:7011";
            httpClient.BaseAddress = new Uri(baseUrl);
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(responseBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (apiResponse?.Success == true)
                    return Json(new { success = true, data = apiResponse.Data, message = apiResponse.Message ?? "Import completed" });
                return Json(new { success = false, message = apiResponse?.Message ?? "Import failed" });
            }

            return Json(new { success = false, message = "Import failed. Please check your file format." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Excel");
            return Json(new { success = false, message = "An error occurred during import" });
        }
    }

    /// <summary>
    /// Export contacts to CSV (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExportCsv([FromBody] List<int>? contactIds)
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
            using var httpClient = new HttpClient();
            var baseUrl = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"] ?? "https://localhost:7011";
            httpClient.BaseAddress = new Uri(baseUrl);
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = System.Text.Json.JsonSerializer.Serialize(contactIds);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/api/contacts/export/csv", content);

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                return File(fileBytes, "text/csv", $"contacts-export-{DateTime.Now:yyyyMMdd}.csv");
            }

            return Json(new { success = false, message = "Export failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting contacts to CSV");
            return Json(new { success = false, message = "An error occurred during export" });
        }
    }

    // ========================================================================
    // DYNAMIC SEGMENTS
    // ========================================================================

    /// <summary>
    /// Segments management page
    /// </summary>
    public IActionResult Segments()
    {
        return View();
    }

    /// <summary>
    /// Evaluate segment criteria (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EvaluateSegment([FromBody] object criteriaData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/audience/evaluate", criteriaData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = result?.Message ?? "Failed to evaluate segment" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating segment");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Calculate audience size (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CalculateAudienceSize([FromBody] object criteriaData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/audience/calculate-size", criteriaData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating audience size");
            return Json(new { success = false, data = 0 });
        }
    }

    /// <summary>
    /// Refresh dynamic group (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RefreshGroup(int groupId)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>($"/api/audience/groups/{groupId}/refresh", new { });
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Done" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing group {GroupId}", groupId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create contact group (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] object groupData)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, ApiResponse<object>>("/api/contactgroups", groupData);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data, message = result.Message ?? "Group created" });
            return Json(new { success = false, message = result?.Message ?? "Failed to create group" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error creating group");
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update contact group (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] object groupData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/contactgroups/{id}", groupData);
            if (result?.Success == true)
                return Json(new { success = true, message = result.Message ?? "Group updated" });
            return Json(new { success = false, message = result?.Message ?? "Failed to update group" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error updating group {GroupId}", id);
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete contact group (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/api/contactgroups/{id}");
            return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Operation completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", id);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    // ========================================================================
    // CONTACT SCORING / ENGAGEMENT
    // ========================================================================

    /// <summary>
    /// Contact scoring dashboard page
    /// </summary>
    public IActionResult Scoring()
    {
        return View();
    }

    /// <summary>
    /// Get contact engagement data (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEngagementData(string? startDate, string? endDate)
    {
        try
        {
            var queryString = "/api/analytics/contacts/engagement?pageNumber=1&pageSize=100";
            if (!string.IsNullOrEmpty(startDate))
                queryString += $"&startDate={Uri.EscapeDataString(startDate)}";
            if (!string.IsNullOrEmpty(endDate))
                queryString += $"&endDate={Uri.EscapeDataString(endDate)}";

            var result = await _apiClient.GetAsync<ApiResponse<object>>(queryString);
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching engagement data");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get single contact engagement details (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactEngagement(int contactId)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/analytics/contacts/{contactId}/engagement");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, message = "No engagement data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching engagement for contact {ContactId}", contactId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update contact (SERVER-SIDE proxy)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] object contactData)
    {
        try
        {
            var result = await _apiClient.PutAsync<object, ApiResponse<object>>($"/api/contacts/{id}", contactData);
            if (result?.Success == true)
                return Json(new { success = true, message = result.Message ?? "Contact updated" });
            return Json(new { success = false, message = result?.Message ?? "Failed to update contact" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API error updating contact {ContactId}", id);
            var msg = ex.Message.StartsWith("API Error:") ? ex.Message.Replace("API Error: ", "") : ex.Message;
            return Json(new { success = false, message = msg });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Search contacts (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchContacts(string searchTerm)
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>($"/api/contacts/search?searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (result?.Success == true)
                return Json(new { success = true, data = result.Data });
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching contacts");
            return Json(new { success = false, data = new object[] { } });
        }
    }

    /// <summary>
    /// Get all tags for dropdown (SERVER-SIDE proxy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        try
        {
            var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/contacttags?pageNumber=1&pageSize=200");
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
            return Json(new { success = false, data = new object[] { } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all tags");
            return Json(new { success = false, data = new object[] { } });
        }
    }
}
