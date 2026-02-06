using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ILandingFeatureService
    {
        Task<IEnumerable<LandingFeature>> GetAllActiveAsync();
        Task<LandingFeature?> GetByIdAsync(int id);
        Task<LandingFeature> CreateAsync(LandingFeature feature);
        Task<LandingFeature> UpdateAsync(int id, LandingFeature feature);
        Task<bool> DeleteAsync(int id);
    }
}
