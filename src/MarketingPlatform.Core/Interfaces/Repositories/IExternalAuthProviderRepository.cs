using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IExternalAuthProviderRepository : IRepository<ExternalAuthProvider>
    {
        Task<ExternalAuthProvider?> GetByNameAsync(string name);
        Task<ExternalAuthProvider?> GetDefaultProviderAsync();
        Task<IEnumerable<ExternalAuthProvider>> GetEnabledProvidersAsync();
        Task<bool> SetDefaultProviderAsync(int providerId);
    }
}
