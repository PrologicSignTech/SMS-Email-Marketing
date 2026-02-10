using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.PhoneNumber;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPhoneNumberService
    {
        Task<PaginatedResult<PhoneNumberDto>> GetAllAsync(PagedRequest request);
        Task<PhoneNumberDto?> GetByIdAsync(int id);
        Task<List<PhoneNumberDto>> GetByUserAsync(string userId);
        Task<List<PhoneNumberDto>> GetAvailableAsync();
        Task<PhoneNumberDto> CreateAsync(CreatePhoneNumberDto dto);
        Task<PhoneNumberDto> UpdateAsync(int id, UpdatePhoneNumberDto dto);
        Task<PhoneNumberDto> AssignAsync(int id, string userId);
        Task<PhoneNumberDto> UnassignAsync(int id);
        Task<PhoneNumberDto> PurchaseAsync(string userId, PurchasePhoneNumberDto dto);
        Task<bool> ReleaseAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
