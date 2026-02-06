using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.ContactTag;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IContactTagService
    {
        Task<ContactTagDto?> GetByIdAsync(string userId, int id);
        Task<PaginatedResult<ContactTagDto>> GetAllAsync(string userId, PagedRequest request);
        Task<ContactTagDto> CreateAsync(string userId, CreateContactTagDto dto);
        Task<bool> UpdateAsync(string userId, int id, CreateContactTagDto dto);
        Task<bool> DeleteAsync(string userId, int id);
        Task<bool> AssignTagToContactAsync(string userId, int contactId, int tagId);
        Task<bool> RemoveTagFromContactAsync(string userId, int contactId, int tagId);
        Task<List<ContactTagDto>> GetContactTagsAsync(string userId, int contactId);
    }
}
