using MarketingPlatform.Application.DTOs.URL;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Application.Interfaces
{
    /// <summary>
    /// Service for URL shortening and click tracking
    /// </summary>
    public interface IUrlShortenerService
    {
        /// <summary>
        /// Create a shortened URL for a campaign
        /// </summary>
        Task<UrlShortenerDto> CreateShortenedUrlAsync(string userId, CreateShortenedUrlDto dto);

        /// <summary>
        /// Get shortened URL by ID
        /// </summary>
        Task<UrlShortenerDto?> GetShortenedUrlByIdAsync(string userId, int id);

        /// <summary>
        /// Get shortened URL by short code
        /// </summary>
        Task<UrlShortenerDto?> GetShortenedUrlByCodeAsync(string shortCode);

        /// <summary>
        /// Get all shortened URLs for a campaign
        /// </summary>
        Task<List<UrlShortenerDto>> GetUrlsByCampaignAsync(string userId, int campaignId);

        /// <summary>
        /// Get paginated list of shortened URLs for user
        /// </summary>
        Task<PaginatedResult<UrlShortenerDto>> GetUrlsAsync(string userId, PagedRequest request);

        /// <summary>
        /// Track a URL click
        /// </summary>
        Task<bool> TrackClickAsync(string shortCode, UrlClickDto clickInfo);

        /// <summary>
        /// Get click statistics for a shortened URL
        /// </summary>
        Task<UrlClickStatsDto> GetClickStatsAsync(string userId, int urlShortenerId);

        /// <summary>
        /// Get click statistics for a campaign
        /// </summary>
        Task<CampaignUrlStatsDto> GetCampaignUrlStatsAsync(string userId, int campaignId);

        /// <summary>
        /// Delete shortened URL
        /// </summary>
        Task<bool> DeleteShortenedUrlAsync(string userId, int id);
    }
}
