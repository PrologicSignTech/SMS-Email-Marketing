using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// Service for campaign-related API operations
/// </summary>
public interface ICampaignApiService
{
    /// <summary>
    /// Get paginated list of campaigns
    /// </summary>
    Task<ApiResponse<PaginatedResult<CampaignDto>>> GetCampaignsAsync(PagedRequest request);

    /// <summary>
    /// Get a specific campaign by ID
    /// </summary>
    Task<ApiResponse<CampaignDto>> GetCampaignByIdAsync(int id);

    /// <summary>
    /// Get campaigns by status
    /// </summary>
    Task<ApiResponse<List<CampaignDto>>> GetCampaignsByStatusAsync(CampaignStatus status);

    /// <summary>
    /// Create a new campaign
    /// </summary>
    Task<ApiResponse<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto);

    /// <summary>
    /// Update an existing campaign
    /// </summary>
    Task<ApiResponse<bool>> UpdateCampaignAsync(int id, UpdateCampaignDto dto);

    /// <summary>
    /// Delete a campaign
    /// </summary>
    Task<ApiResponse<bool>> DeleteCampaignAsync(int id);

    /// <summary>
    /// Start a campaign
    /// </summary>
    Task<ApiResponse<bool>> StartCampaignAsync(int id);

    /// <summary>
    /// Pause a campaign
    /// </summary>
    Task<ApiResponse<bool>> PauseCampaignAsync(int id);

    /// <summary>
    /// Resume a paused campaign
    /// </summary>
    Task<ApiResponse<bool>> ResumeCampaignAsync(int id);

    /// <summary>
    /// Archive a campaign
    /// </summary>
    Task<ApiResponse<bool>> ArchiveCampaignAsync(int id);
}
