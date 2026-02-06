using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using Microsoft.AspNetCore.Http;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IContactService
    {
        Task<ContactDto?> GetContactByIdAsync(string userId, int contactId);
        Task<PaginatedResult<ContactDto>> GetContactsAsync(string userId, PagedRequest request);
        Task<ContactDto> CreateContactAsync(string userId, CreateContactDto dto);
        Task<bool> UpdateContactAsync(string userId, int contactId, UpdateContactDto dto);
        Task<bool> DeleteContactAsync(string userId, int contactId);
        Task<bool> DeleteContactsAsync(string userId, List<int> contactIds);
        Task<ContactImportResultDto> ImportContactsFromCsvAsync(string userId, IFormFile file, int? groupId = null);
        Task<ContactImportResultDto> ImportContactsFromExcelAsync(string userId, IFormFile file, int? groupId = null);
        Task<byte[]> ExportContactsToCsvAsync(string userId, List<int>? contactIds = null);
        Task<byte[]> ExportContactsToExcelAsync(string userId, List<int>? contactIds = null);
        Task<List<ContactDto>> SearchContactsAsync(string userId, string searchTerm);
        
        // Duplicate detection & handling
        Task<DuplicateCheckResultDto> CheckForDuplicatesAsync(string userId, CheckDuplicateDto dto, int? excludeContactId = null);
        Task<DuplicateReportDto> GetDuplicateReportAsync(string userId);
        Task<ResolveDuplicateResultDto> ResolveDuplicatesAsync(string userId, ResolveDuplicateDto dto);
    }
}
