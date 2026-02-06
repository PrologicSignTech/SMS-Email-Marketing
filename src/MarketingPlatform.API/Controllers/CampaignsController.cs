using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly ILogger<CampaignsController> _logger;

        public CampaignsController(ICampaignService campaignService, ILogger<CampaignsController> logger)
        {
            _campaignService = campaignService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<CampaignDto>>>> GetCampaigns([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaigns = await _campaignService.GetCampaignsAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<CampaignDto>>.SuccessResponse(campaigns));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CampaignDto>>> GetCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaign = await _campaignService.GetCampaignByIdAsync(userId, id);
            if (campaign == null)
                return NotFound(ApiResponse<CampaignDto>.ErrorResponse("Campaign not found"));

            return Ok(ApiResponse<CampaignDto>.SuccessResponse(campaign));
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<CampaignDto>>>> GetCampaignsByStatus(CampaignStatus status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaigns = await _campaignService.GetCampaignsByStatusAsync(userId, status);
            return Ok(ApiResponse<List<CampaignDto>>.SuccessResponse(campaigns));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CampaignDto>>> CreateCampaign([FromBody] CreateCampaignDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var campaign = await _campaignService.CreateCampaignAsync(userId, dto);
                return Ok(ApiResponse<CampaignDto>.SuccessResponse(campaign, "Campaign created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                return BadRequest(ApiResponse<CampaignDto>.ErrorResponse("Failed to create campaign", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCampaign(int id, [FromBody] UpdateCampaignDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.UpdateCampaignAsync(userId, id, dto);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update campaign. Campaign not found or not in Draft status."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.DeleteCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete campaign. Campaign not found or is currently running."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign deleted successfully"));
        }

        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<ApiResponse<bool>>> DuplicateCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.DuplicateCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to duplicate campaign. Campaign not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign duplicated successfully"));
        }

        [HttpPost("{id}/schedule")]
        public async Task<ActionResult<ApiResponse<bool>>> ScheduleCampaign(int id, [FromBody] DateTime scheduledDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.ScheduleCampaignAsync(userId, id, scheduledDate);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to schedule campaign. Campaign not found or not in Draft status."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign scheduled successfully"));
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult<ApiResponse<bool>>> StartCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.StartCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to start campaign. Campaign not found or not in Draft/Scheduled status."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign started successfully"));
        }

        [HttpPost("{id}/pause")]
        public async Task<ActionResult<ApiResponse<bool>>> PauseCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.PauseCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to pause campaign. Campaign not found or not running."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign paused successfully"));
        }

        [HttpPost("{id}/resume")]
        public async Task<ActionResult<ApiResponse<bool>>> ResumeCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.ResumeCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to resume campaign. Campaign not found or not paused."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign resumed successfully"));
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelCampaign(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _campaignService.CancelCampaignAsync(userId, id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel campaign. Campaign not found or already completed/failed."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign cancelled successfully"));
        }

        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<CampaignStatsDto>>> GetCampaignStats(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var stats = await _campaignService.GetCampaignStatsAsync(userId, id);
                return Ok(ApiResponse<CampaignStatsDto>.SuccessResponse(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign stats");
                return BadRequest(ApiResponse<CampaignStatsDto>.ErrorResponse("Failed to get campaign statistics", new List<string> { ex.Message }));
            }
        }

        // A/B Testing endpoints
        [HttpGet("{campaignId}/variants")]
        public async Task<ActionResult<ApiResponse<List<CampaignVariantDto>>>> GetCampaignVariants(int campaignId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var variants = await abTestingService.GetCampaignVariantsAsync(userId, campaignId);
            return Ok(ApiResponse<List<CampaignVariantDto>>.SuccessResponse(variants));
        }

        [HttpGet("{campaignId}/variants/{variantId}")]
        public async Task<ActionResult<ApiResponse<CampaignVariantDto>>> GetVariant(int campaignId, int variantId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var variant = await abTestingService.GetVariantByIdAsync(userId, campaignId, variantId);
            if (variant == null)
                return NotFound(ApiResponse<CampaignVariantDto>.ErrorResponse("Variant not found"));

            return Ok(ApiResponse<CampaignVariantDto>.SuccessResponse(variant));
        }

        [HttpPost("{campaignId}/variants")]
        public async Task<ActionResult<ApiResponse<CampaignVariantDto>>> CreateVariant(int campaignId, [FromBody] CreateCampaignVariantDto dto, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var variant = await abTestingService.CreateVariantAsync(userId, campaignId, dto);
                return Ok(ApiResponse<CampaignVariantDto>.SuccessResponse(variant, "Variant created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating variant");
                return BadRequest(ApiResponse<CampaignVariantDto>.ErrorResponse("Failed to create variant", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{campaignId}/variants/{variantId}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateVariant(int campaignId, int variantId, [FromBody] UpdateCampaignVariantDto dto, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await abTestingService.UpdateVariantAsync(userId, campaignId, variantId, dto);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update variant. Variant not found or campaign is not in Draft status."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Variant updated successfully"));
        }

        [HttpDelete("{campaignId}/variants/{variantId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteVariant(int campaignId, int variantId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await abTestingService.DeleteVariantAsync(userId, campaignId, variantId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete variant. Variant not found or campaign is not in Draft status."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Variant deleted successfully"));
        }

        [HttpPost("{campaignId}/variants/{variantId}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateVariant(int campaignId, int variantId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await abTestingService.ActivateVariantAsync(userId, campaignId, variantId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to activate variant."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Variant activated successfully"));
        }

        [HttpPost("{campaignId}/variants/{variantId}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateVariant(int campaignId, int variantId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await abTestingService.DeactivateVariantAsync(userId, campaignId, variantId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to deactivate variant."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Variant deactivated successfully"));
        }

        [HttpGet("{campaignId}/variants/comparison")]
        public async Task<ActionResult<ApiResponse<VariantComparisonDto>>> CompareVariants(int campaignId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var comparison = await abTestingService.CompareVariantsAsync(userId, campaignId);
            return Ok(ApiResponse<VariantComparisonDto>.SuccessResponse(comparison));
        }

        [HttpPost("{campaignId}/variants/{variantId}/select-winner")]
        public async Task<ActionResult<ApiResponse<bool>>> SelectWinningVariant(int campaignId, int variantId, [FromServices] ICampaignABTestingService abTestingService)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await abTestingService.SelectWinningVariantAsync(userId, campaignId, variantId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to select winning variant."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Winning variant selected successfully"));
        }

        [HttpPost("calculate-audience")]
        public async Task<ActionResult<ApiResponse<int>>> CalculateAudienceSize([FromBody] CampaignAudienceDto audience)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var size = await _campaignService.CalculateAudienceSizeAsync(userId, audience);
            return Ok(ApiResponse<int>.SuccessResponse(size));
        }
    }
}
