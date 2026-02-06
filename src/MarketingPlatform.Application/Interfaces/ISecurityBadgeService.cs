using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISecurityBadgeService
    {
        Task<IEnumerable<SecurityBadge>> GetAllActiveAsync();
        Task<SecurityBadge?> GetByIdAsync(int id);
        Task<SecurityBadge> CreateAsync(SecurityBadge badge);
        Task<SecurityBadge> UpdateAsync(int id, SecurityBadge badge);
        Task<bool> DeleteAsync(int id);
    }
}
