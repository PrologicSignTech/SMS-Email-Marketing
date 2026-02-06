using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Integration;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly ICRMIntegrationService _crmIntegrationService;
        private readonly ILogger<IntegrationController> _logger;

        public IntegrationController(
            ICRMIntegrationService crmIntegrationService,
            ILogger<IntegrationController> logger)
        {
            _crmIntegrationService = crmIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// Test CRM connection with provided configuration
        /// </summary>
        [HttpPost("crm/test-connection")]
        public async Task<ActionResult<ApiResponse<CRMConnectionTestResultDto>>> TestCRMConnection(
            [FromBody] CRMSyncConfigDto config)
        {
            try
            {
                var result = await _crmIntegrationService.TestConnectionAsync(config);
                return Ok(ApiResponse<CRMConnectionTestResultDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing CRM connection");
                return BadRequest(ApiResponse<CRMConnectionTestResultDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get available CRM fields for mapping
        /// </summary>
        [HttpPost("crm/fields")]
        public async Task<ActionResult<ApiResponse<List<CRMFieldDto>>>> GetCRMFields(
            [FromBody] CRMSyncConfigDto config)
        {
            try
            {
                var fields = await _crmIntegrationService.GetCRMFieldsAsync(config);
                return Ok(ApiResponse<List<CRMFieldDto>>.SuccessResponse(fields));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CRM fields");
                return BadRequest(ApiResponse<List<CRMFieldDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Sync contacts from CRM to platform
        /// </summary>
        [HttpPost("crm/sync-from")]
        public async Task<ActionResult<ApiResponse<CRMSyncResultDto>>> SyncContactsFromCRM(
            [FromBody] CRMSyncConfigDto config)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _crmIntegrationService.SyncContactsFromCRMAsync(userId, config);
                
                if (result.Success)
                {
                    return Ok(ApiResponse<CRMSyncResultDto>.SuccessResponse(result, result.Message));
                }
                else
                {
                    return BadRequest(ApiResponse<CRMSyncResultDto>.ErrorResponse(result.Message, result.Errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing contacts from CRM");
                return BadRequest(ApiResponse<CRMSyncResultDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Sync contacts from platform to CRM
        /// </summary>
        [HttpPost("crm/sync-to")]
        public async Task<ActionResult<ApiResponse<CRMSyncResultDto>>> SyncContactsToCRM(
            [FromBody] SyncToCRMRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _crmIntegrationService.SyncContactsToCRMAsync(
                    userId, 
                    request.ContactIds, 
                    request.Config);
                
                if (result.Success)
                {
                    return Ok(ApiResponse<CRMSyncResultDto>.SuccessResponse(result, result.Message));
                }
                else
                {
                    return BadRequest(ApiResponse<CRMSyncResultDto>.ErrorResponse(result.Message, result.Errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing contacts to CRM");
                return BadRequest(ApiResponse<CRMSyncResultDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Sync campaign results to CRM
        /// </summary>
        [HttpPost("crm/sync-campaign/{campaignId}")]
        public async Task<ActionResult<ApiResponse<bool>>> SyncCampaignResultsToCRM(
            int campaignId,
            [FromBody] CRMSyncConfigDto config)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _crmIntegrationService.SyncCampaignResultsToCRMAsync(
                    userId, 
                    campaignId, 
                    config);
                
                if (result)
                {
                    return Ok(ApiResponse<bool>.SuccessResponse(true, "Campaign results synced successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to sync campaign results"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing campaign results to CRM");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }

    public class SyncToCRMRequestDto
    {
        public List<int> ContactIds { get; set; } = new();
        public CRMSyncConfigDto Config { get; set; } = new();
    }
}
