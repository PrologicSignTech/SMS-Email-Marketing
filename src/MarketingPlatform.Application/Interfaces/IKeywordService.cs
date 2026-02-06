using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Keyword;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IKeywordService
    {
        Task<KeywordDto?> GetKeywordByIdAsync(string userId, int keywordId);
        Task<PaginatedResult<KeywordDto>> GetKeywordsAsync(string userId, PagedRequest request);
        Task<List<KeywordDto>> GetKeywordsByStatusAsync(string userId, KeywordStatus status);
        Task<KeywordDto> CreateKeywordAsync(string userId, CreateKeywordDto dto);
        Task<bool> UpdateKeywordAsync(string userId, int keywordId, UpdateKeywordDto dto);
        Task<bool> DeleteKeywordAsync(string userId, int keywordId);
        Task<bool> CheckKeywordAvailabilityAsync(string keywordText, string userId);
        Task<PaginatedResult<KeywordActivityDto>> GetKeywordActivitiesAsync(int keywordId, string userId, PagedRequest request);
        Task<KeywordActivityDto> ProcessInboundKeywordAsync(string phoneNumber, string message);
        Task<int> GetKeywordActivityCountAsync(int keywordId);
        Task<KeywordAnalyticsDto?> GetKeywordAnalyticsAsync(int keywordId, string userId);

        // Keyword Reservation Methods (12.4.1)
        Task<KeywordReservationDto> CreateReservationAsync(string userId, CreateKeywordReservationDto dto);
        Task<KeywordReservationDto?> GetReservationByIdAsync(int reservationId);
        Task<PaginatedResult<KeywordReservationDto>> GetReservationsAsync(PagedRequest request);
        Task<bool> UpdateReservationAsync(string userId, int reservationId, UpdateKeywordReservationDto dto);
        Task<bool> ApproveReservationAsync(string approverUserId, int reservationId);
        Task<bool> RejectReservationAsync(string approverUserId, int reservationId, string reason);

        // Keyword Assignment Methods (12.4.2)
        Task<KeywordAssignmentDto> AssignKeywordToCampaignAsync(string userId, CreateKeywordAssignmentDto dto);
        Task<KeywordAssignmentDto?> GetAssignmentByIdAsync(int assignmentId);
        Task<PaginatedResult<KeywordAssignmentDto>> GetAssignmentsAsync(PagedRequest request);
        Task<List<KeywordAssignmentDto>> GetAssignmentsByCampaignAsync(int campaignId);
        Task<bool> UnassignKeywordAsync(string userId, int assignmentId);

        // Keyword Conflict Resolution Methods (12.4.3)
        Task<KeywordConflictDto?> CheckForConflictAsync(string keywordText, string requestingUserId);
        Task<PaginatedResult<KeywordConflictDto>> GetConflictsAsync(PagedRequest request);
        Task<bool> ResolveConflictAsync(string resolverUserId, int conflictId, ResolveKeywordConflictDto dto);
    }
}
