using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionList;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISuppressionListService
    {
        Task<SuppressionListDto?> GetByIdAsync(string userId, int id);
        Task<PaginatedResult<SuppressionListDto>> GetAllAsync(string userId, PagedRequest request);
        Task<SuppressionListDto> CreateAsync(string userId, CreateSuppressionListDto dto);
        Task<bool> DeleteAsync(string userId, int id);
        Task<int> BulkCreateAsync(string userId, BulkSuppressionDto dto);
        Task<bool> IsSuppressedAsync(string userId, string phoneOrEmail);
    }
}
