using MarketingPlatform.Application.DTOs.Audience;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IAudienceSegmentationService
    {
        Task<AudienceSegmentDto> EvaluateSegmentAsync(string userId, SegmentCriteriaDto criteria);
        Task<int> CalculateAudienceSizeAsync(string userId, SegmentCriteriaDto criteria);
        Task<bool> UpdateDynamicGroupMembersAsync(string userId, int groupId);
    }
}
