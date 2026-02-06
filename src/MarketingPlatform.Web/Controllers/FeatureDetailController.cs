using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MarketingPlatform.Web.Controllers
{
    /// <summary>
    /// Controller for displaying landing feature detail pages
    /// </summary>
    public class FeatureDetailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeatureDetailController> _logger;
        private readonly HttpClient _httpClient;

        public FeatureDetailController(
            IConfiguration configuration,
            ILogger<FeatureDetailController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Display feature detail page
        /// </summary>
        /// <param name="id">Feature ID</param>
        [HttpGet("/features/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/landingfeatures/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Feature {FeatureId} not found. Status: {Status}", id, response.StatusCode);
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true && result.Data != null)
                {
                    ViewBag.Feature = result.Data;
                    return View();
                }

                _logger.LogWarning("Feature {FeatureId} returned no data", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading feature detail {FeatureId}", id);
                return StatusCode(500, "Error loading feature details");
            }
        }

        // API Response DTOs
        private class ApiResponse
        {
            public bool Success { get; set; }
            public FeatureData? Data { get; set; }
            public string? Message { get; set; }
        }

        public class FeatureData
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string ShortDescription { get; set; } = string.Empty;
            public string DetailedDescription { get; set; } = string.Empty;
            public string IconClass { get; set; } = string.Empty;
            public string ColorClass { get; set; } = string.Empty;
            public string? HeaderImageUrl { get; set; }
            public string? VideoUrl { get; set; }
            public string? GalleryImages { get; set; }
            public string? ContactName { get; set; }
            public string? ContactEmail { get; set; }
            public string? ContactPhone { get; set; }
            public string? ContactMessage { get; set; }
            public string? StatTitle1 { get; set; }
            public string? StatValue1 { get; set; }
            public string? StatTitle2 { get; set; }
            public string? StatValue2 { get; set; }
            public string? StatTitle3 { get; set; }
            public string? StatValue3 { get; set; }
        }
    }
}
