using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ITrustedCompanyService
    {
        Task<IEnumerable<TrustedCompany>> GetAllActiveAsync();
        Task<TrustedCompany?> GetByIdAsync(int id);
        Task<TrustedCompany> CreateAsync(TrustedCompany company);
        Task<TrustedCompany> UpdateAsync(int id, TrustedCompany company);
        Task<bool> DeleteAsync(int id);
    }
}
