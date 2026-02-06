using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ILandingFaqService
    {
        Task<IEnumerable<LandingFaq>> GetAllActiveAsync();
        Task<LandingFaq?> GetByIdAsync(int id);
        Task<LandingFaq> CreateAsync(LandingFaq faq);
        Task<LandingFaq> UpdateAsync(int id, LandingFaq faq);
        Task<bool> DeleteAsync(int id);
    }
}
