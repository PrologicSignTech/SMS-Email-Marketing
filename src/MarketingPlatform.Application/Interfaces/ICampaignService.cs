using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ICampaignService
    {
        Task<CampaignDto?> GetCampaignByIdAsync(string userId, int campaignId);
        Task<PaginatedResult<CampaignDto>> GetCampaignsAsync(string userId, PagedRequest request);
        Task<List<CampaignDto>> GetCampaignsByStatusAsync(string userId, CampaignStatus status);
        Task<CampaignDto> CreateCampaignAsync(string userId, CreateCampaignDto dto);
        Task<bool> UpdateCampaignAsync(string userId, int campaignId, UpdateCampaignDto dto);
        Task<bool> DeleteCampaignAsync(string userId, int campaignId);
        Task<bool> DuplicateCampaignAsync(string userId, int campaignId);
        Task<bool> ScheduleCampaignAsync(string userId, int campaignId, DateTime scheduledDate);
        Task<bool> StartCampaignAsync(string userId, int campaignId);
        Task<bool> PauseCampaignAsync(string userId, int campaignId);
        Task<bool> ResumeCampaignAsync(string userId, int campaignId);
        Task<bool> CancelCampaignAsync(string userId, int campaignId);
        Task<CampaignStatsDto> GetCampaignStatsAsync(string userId, int campaignId);
        Task<int> CalculateAudienceSizeAsync(string userId, CampaignAudienceDto audience);
    }
}
