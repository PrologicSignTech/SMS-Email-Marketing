using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IFooterSettingsService
    {
        Task<FooterSettings?> GetActiveAsync();
        Task<FooterSettings?> GetByIdAsync(int id);
        Task<FooterSettings> CreateAsync(FooterSettings settings);
        Task<FooterSettings> UpdateAsync(int id, FooterSettings settings);
    }
}
