using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Provider;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IProviderService
    {
        Task<PaginatedResult<ProviderDto>> GetProvidersAsync(PagedRequest request);
        Task<ProviderDetailDto?> GetProviderByIdAsync(int id);
        Task<ProviderDto> CreateProviderAsync(CreateProviderDto dto);
        Task<bool> UpdateProviderAsync(int id, UpdateProviderDto dto);
        Task<bool> DeleteProviderAsync(int id);
        Task<bool> TestProviderAsync(int id);
        Task<List<string>> GetChannelTypesAsync();
    }
}
