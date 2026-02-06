using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ILandingStatService
    {
        Task<IEnumerable<LandingStat>> GetAllActiveAsync();
        Task<LandingStat?> GetByIdAsync(int id);
        Task<LandingStat> CreateAsync(LandingStat stat);
        Task<LandingStat> UpdateAsync(int id, LandingStat stat);
        Task<bool> DeleteAsync(int id);
    }
}
