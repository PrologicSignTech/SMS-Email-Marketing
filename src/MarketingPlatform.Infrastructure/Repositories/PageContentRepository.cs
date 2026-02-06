using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class PageContentRepository : Repository<PageContent>, IPageContentRepository
    {
        public PageContentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PageContent?> GetByPageKeyAsync(string pageKey)
        {
            return await _dbSet
                .Include(p => p.LastModifiedByUser)
                .FirstOrDefaultAsync(p => p.PageKey == pageKey);
        }

        public async Task<bool> ExistsByPageKeyAsync(string pageKey)
        {
            return await _dbSet.AnyAsync(p => p.PageKey == pageKey);
        }
    }
}
