using Microsoft.EntityFrameworkCore;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class ExternalAuthProviderRepository : Repository<ExternalAuthProvider>, IExternalAuthProviderRepository
    {
        public ExternalAuthProviderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ExternalAuthProvider?> GetByNameAsync(string name)
        {
            return await _context.ExternalAuthProviders
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<ExternalAuthProvider?> GetDefaultProviderAsync()
        {
            return await _context.ExternalAuthProviders
                .FirstOrDefaultAsync(p => p.IsDefault && p.IsEnabled);
        }

        public async Task<IEnumerable<ExternalAuthProvider>> GetEnabledProvidersAsync()
        {
            return await _context.ExternalAuthProviders
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.DisplayName)
                .ToListAsync();
        }

        public async Task<bool> SetDefaultProviderAsync(int providerId)
        {
            // Remove default flag from all providers
            var allProviders = await _context.ExternalAuthProviders.ToListAsync();
            foreach (var provider in allProviders)
            {
                provider.IsDefault = provider.Id == providerId;
                provider.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
