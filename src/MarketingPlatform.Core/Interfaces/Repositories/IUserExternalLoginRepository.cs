using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IUserExternalLoginRepository : IRepository<UserExternalLogin>
    {
        Task<UserExternalLogin?> GetByProviderAsync(string providerName, string providerUserId);
        Task<IEnumerable<UserExternalLogin>> GetByUserIdAsync(string userId);
        Task<bool> RemoveByProviderAsync(string userId, string providerName);
    }
}
