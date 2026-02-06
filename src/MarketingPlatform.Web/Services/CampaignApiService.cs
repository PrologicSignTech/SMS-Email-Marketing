using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Web.Services;

/// <summary>
/// Implementation of campaign API service
/// </summary>
public class CampaignApiService : ICampaignApiService
{
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CampaignApiService> _logger;

    public CampaignApiService(
        IApiClient apiClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CampaignApiService> logger)
    {
        _apiClient = apiClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResult<CampaignDto>>> GetCampaignsAsync(PagedRequest request)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.GetAsync<ApiResponse<PaginatedResult<CampaignDto>>>(
                $"/campaigns?pageNumber={request.PageNumber}&pageSize={request.PageSize}" +
                $"&searchTerm={request.SearchTerm}&sortBy={request.SortBy}&sortDescending={request.SortDescending}");

            return response ?? ApiResponse<PaginatedResult<CampaignDto>>.ErrorResponse("Failed to get campaigns");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaigns");
            return ApiResponse<PaginatedResult<CampaignDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CampaignDto>> GetCampaignByIdAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.GetAsync<ApiResponse<CampaignDto>>($"/campaigns/{id}");
            return response ?? ApiResponse<CampaignDto>.ErrorResponse("Campaign not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign {CampaignId}", id);
            return ApiResponse<CampaignDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<CampaignDto>>> GetCampaignsByStatusAsync(CampaignStatus status)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.GetAsync<ApiResponse<List<CampaignDto>>>(
                $"/campaigns/status/{(int)status}");

            return response ?? ApiResponse<List<CampaignDto>>.ErrorResponse("Failed to get campaigns by status");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaigns by status {Status}", status);
            return ApiResponse<List<CampaignDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PostAsync<CreateCampaignDto, ApiResponse<CampaignDto>>(
                "/campaigns",
                dto);

            return response ?? ApiResponse<CampaignDto>.ErrorResponse("Failed to create campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return ApiResponse<CampaignDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> UpdateCampaignAsync(int id, UpdateCampaignDto dto)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PutAsync<UpdateCampaignDto, ApiResponse<bool>>(
                $"/campaigns/{id}",
                dto);

            return response ?? ApiResponse<bool>.ErrorResponse("Failed to update campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCampaignAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.DeleteAsync<ApiResponse<bool>>($"/campaigns/{id}");
            return response ?? ApiResponse<bool>.ErrorResponse("Failed to delete campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> StartCampaignAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PostAsync<object, ApiResponse<bool>>(
                $"/campaigns/{id}/start",
                new { });

            return response ?? ApiResponse<bool>.ErrorResponse("Failed to start campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> PauseCampaignAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PostAsync<object, ApiResponse<bool>>(
                $"/campaigns/{id}/pause",
                new { });

            return response ?? ApiResponse<bool>.ErrorResponse("Failed to pause campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ResumeCampaignAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PostAsync<object, ApiResponse<bool>>(
                $"/campaigns/{id}/resume",
                new { });

            return response ?? ApiResponse<bool>.ErrorResponse("Failed to resume campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ArchiveCampaignAsync(int id)
    {
        try
        {
            EnsureAuthorization();

            var response = await _apiClient.PostAsync<object, ApiResponse<bool>>(
                $"/campaigns/{id}/archive",
                new { });

            return response ?? ApiResponse<bool>.ErrorResponse("Failed to archive campaign");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving campaign {CampaignId}", id);
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    private void EnsureAuthorization()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
            var token = httpContext.User.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _apiClient.SetAuthorizationToken(token);
            }
        }
    }
}
