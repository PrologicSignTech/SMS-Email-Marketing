using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.DTOs.ContactGroup;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IContactGroupService
    {
        Task<ContactGroupDto?> GetGroupByIdAsync(string userId, int groupId);
        Task<PaginatedResult<ContactGroupDto>> GetGroupsAsync(string userId, PagedRequest request);
        Task<ContactGroupDto> CreateGroupAsync(string userId, CreateContactGroupDto dto);
        Task<bool> UpdateGroupAsync(string userId, int groupId, CreateContactGroupDto dto);
        Task<bool> DeleteGroupAsync(string userId, int groupId);
        Task<bool> AddContactToGroupAsync(string userId, int groupId, int contactId);
        Task<bool> RemoveContactFromGroupAsync(string userId, int groupId, int contactId);
        Task<bool> AddContactsToGroupAsync(string userId, int groupId, List<int> contactIds);
        Task<bool> RemoveContactsFromGroupAsync(string userId, int groupId, List<int> contactIds);
        Task<PaginatedResult<ContactDto>> GetGroupContactsAsync(string userId, int groupId, PagedRequest request);
    }
}
