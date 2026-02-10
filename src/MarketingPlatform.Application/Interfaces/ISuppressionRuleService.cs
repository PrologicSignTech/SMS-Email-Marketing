using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionRule;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISuppressionRuleService
    {
        Task<PaginatedResult<SuppressionRuleDto>> GetAllAsync(string userId, PagedRequest request);
        Task<SuppressionRuleDto?> GetByIdAsync(string userId, int id);
        Task<List<SuppressionRuleDto>> GetActiveRulesAsync(string userId);
        Task<SuppressionRuleDto> CreateAsync(string userId, CreateSuppressionRuleDto dto);
        Task<SuppressionRuleDto> UpdateAsync(string userId, int id, UpdateSuppressionRuleDto dto);
        Task<bool> ToggleAsync(string userId, int id);
        Task<bool> DeleteAsync(string userId, int id);
        Task SeedDefaultRulesAsync(string userId);
    }
}
