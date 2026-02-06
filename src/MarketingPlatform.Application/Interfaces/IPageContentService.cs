using MarketingPlatform.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPageContentService
    {
        /// <summary>
        /// Get page content by page key (e.g., "privacy-policy", "terms-of-service")
        /// </summary>
        Task<PageContentDto?> GetByPageKeyAsync(string pageKey);

        /// <summary>
        /// Get all page contents
        /// </summary>
        Task<IEnumerable<PageContentDto>> GetAllAsync();

        /// <summary>
        /// Create or update page content
        /// </summary>
        Task<PageContentDto> SavePageContentAsync(PageContentDto dto, string userId);

        /// <summary>
        /// Upload an image for a page
        /// </summary>
        Task<string> UploadImageAsync(IFormFile file, string pageKey);

        /// <summary>
        /// Delete an image from a page
        /// </summary>
        Task<bool> DeleteImageAsync(string imageUrl, string pageKey);
    }
}
