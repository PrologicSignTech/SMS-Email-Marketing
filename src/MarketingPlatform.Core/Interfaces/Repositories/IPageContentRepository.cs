using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IPageContentRepository : IRepository<PageContent>
    {
        /// <summary>
        /// Get page content by its unique key
        /// </summary>
        Task<PageContent?> GetByPageKeyAsync(string pageKey);

        /// <summary>
        /// Check if a page with the given key exists
        /// </summary>
        Task<bool> ExistsByPageKeyAsync(string pageKey);
    }
}
