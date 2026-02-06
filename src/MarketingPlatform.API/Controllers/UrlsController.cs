using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.URL;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly ILogger<UrlsController> _logger;

        public UrlsController(IUrlShortenerService urlShortenerService, ILogger<UrlsController> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }

        /// <summary>
        /// Create a shortened URL for a campaign
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UrlShortenerDto>>> CreateShortenedUrl([FromBody] CreateShortenedUrlDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _urlShortenerService.CreateShortenedUrlAsync(userId, dto);
                return Ok(ApiResponse<UrlShortenerDto>.SuccessResponse(result, "Shortened URL created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UrlShortenerDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UrlShortenerDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shortened URL");
                return BadRequest(ApiResponse<UrlShortenerDto>.ErrorResponse("Failed to create shortened URL", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get all shortened URLs (paginated)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<UrlShortenerDto>>>> GetUrls([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var urls = await _urlShortenerService.GetUrlsAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<UrlShortenerDto>>.SuccessResponse(urls));
        }

        /// <summary>
        /// Get shortened URL by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UrlShortenerDto>>> GetUrl(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var url = await _urlShortenerService.GetShortenedUrlByIdAsync(userId, id);
            if (url == null)
                return NotFound(ApiResponse<UrlShortenerDto>.ErrorResponse("Shortened URL not found"));

            return Ok(ApiResponse<UrlShortenerDto>.SuccessResponse(url));
        }

        /// <summary>
        /// Get all shortened URLs for a campaign
        /// </summary>
        [HttpGet("campaign/{campaignId}")]
        public async Task<ActionResult<ApiResponse<List<UrlShortenerDto>>>> GetUrlsByCampaign(int campaignId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var urls = await _urlShortenerService.GetUrlsByCampaignAsync(userId, campaignId);
                return Ok(ApiResponse<List<UrlShortenerDto>>.SuccessResponse(urls));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<UrlShortenerDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URLs for campaign {CampaignId}", campaignId);
                return BadRequest(ApiResponse<List<UrlShortenerDto>>.ErrorResponse("Failed to get URLs", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get click statistics for a shortened URL
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<UrlClickStatsDto>>> GetClickStats(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var stats = await _urlShortenerService.GetClickStatsAsync(userId, id);
                return Ok(ApiResponse<UrlClickStatsDto>.SuccessResponse(stats));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UrlClickStatsDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting click stats for URL {Id}", id);
                return BadRequest(ApiResponse<UrlClickStatsDto>.ErrorResponse("Failed to get click stats", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get URL statistics for a campaign
        /// </summary>
        [HttpGet("campaign/{campaignId}/stats")]
        public async Task<ActionResult<ApiResponse<CampaignUrlStatsDto>>> GetCampaignUrlStats(int campaignId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var stats = await _urlShortenerService.GetCampaignUrlStatsAsync(userId, campaignId);
                return Ok(ApiResponse<CampaignUrlStatsDto>.SuccessResponse(stats));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CampaignUrlStatsDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign URL stats for campaign {CampaignId}", campaignId);
                return BadRequest(ApiResponse<CampaignUrlStatsDto>.ErrorResponse("Failed to get campaign URL stats", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a shortened URL
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUrl(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _urlShortenerService.DeleteShortenedUrlAsync(userId, id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Shortened URL not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Shortened URL deleted successfully"));
        }

        /// <summary>
        /// Redirect to original URL and track click (public endpoint)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/r/{shortCode}")]
        public async Task<IActionResult> RedirectToUrl(string shortCode)
        {
            try
            {
                var url = await _urlShortenerService.GetShortenedUrlByCodeAsync(shortCode);
                if (url == null)
                    return NotFound("Shortened URL not found");

                // Track the click
                var clickInfo = new UrlClickDto
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Referrer = Request.Headers["Referer"].ToString()
                };

                await _urlShortenerService.TrackClickAsync(shortCode, clickInfo);

                // Redirect to original URL
                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redirecting short URL {ShortCode}", shortCode);
                return NotFound("Shortened URL not found");
            }
        }
    }
}
