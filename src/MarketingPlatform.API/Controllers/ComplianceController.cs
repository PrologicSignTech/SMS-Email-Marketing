using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Compliance;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;
        private readonly ILogger<ComplianceController> _logger;

        public ComplianceController(IComplianceService complianceService, ILogger<ComplianceController> logger)
        {
            _complianceService = complianceService;
            _logger = logger;
        }

        #region Consent Management

        /// <summary>
        /// Get consent status for a specific contact
        /// </summary>
        [HttpGet("contacts/{contactId}/consent-status")]
        public async Task<ActionResult<ApiResponse<ConsentStatusDto>>> GetContactConsentStatus(int contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.GetContactConsentStatusAsync(userId, contactId);
            if (result == null)
                return NotFound(ApiResponse<ConsentStatusDto>.ErrorResponse("Contact not found"));

            return Ok(ApiResponse<ConsentStatusDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Record consent (opt-in or opt-out) for a contact
        /// </summary>
        [HttpPost("consent")]
        public async Task<ActionResult<ApiResponse<ContactConsentDto>>> RecordConsent([FromBody] ConsentRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.RecordConsentAsync(userId, request);
                return Ok(ApiResponse<ContactConsentDto>.SuccessResponse(result, "Consent recorded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording consent");
                return BadRequest(ApiResponse<ContactConsentDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Record consent for multiple contacts in bulk
        /// </summary>
        [HttpPost("consent/bulk")]
        public async Task<ActionResult<ApiResponse<int>>> BulkRecordConsent([FromBody] BulkConsentRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.BulkRecordConsentAsync(userId, request);
                return Ok(ApiResponse<int>.SuccessResponse(result, $"Consent recorded for {result} contacts"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording bulk consent");
                return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Revoke consent (opt-out) for a contact on a specific channel
        /// </summary>
        [HttpPost("contacts/{contactId}/revoke-consent")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeConsent(int contactId, [FromQuery] ConsentChannel channel, [FromQuery] string? reason = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.RevokeConsentAsync(userId, contactId, channel, reason);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Consent revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking consent");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get all consent records for a contact
        /// </summary>
        [HttpGet("contacts/{contactId}/consents")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ContactConsentDto>>>> GetContactConsents(
            int contactId, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.GetContactConsentsAsync(userId, contactId, request);
                return Ok(ApiResponse<PaginatedResult<ContactConsentDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact consents");
                return BadRequest(ApiResponse<PaginatedResult<ContactConsentDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get consent history for a contact
        /// </summary>
        [HttpGet("contacts/{contactId}/consent-history")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ConsentHistoryDto>>>> GetConsentHistory(
            int contactId, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.GetConsentHistoryAsync(userId, contactId, request);
                return Ok(ApiResponse<PaginatedResult<ConsentHistoryDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consent history");
                return BadRequest(ApiResponse<PaginatedResult<ConsentHistoryDto>>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Compliance Settings

        /// <summary>
        /// Get compliance settings for the current user
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<ApiResponse<ComplianceSettingsDto>>> GetComplianceSettings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.GetComplianceSettingsAsync(userId);
            return Ok(ApiResponse<ComplianceSettingsDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Update compliance settings
        /// </summary>
        [HttpPut("settings")]
        public async Task<ActionResult<ApiResponse<ComplianceSettingsDto>>> UpdateComplianceSettings(
            [FromBody] UpdateComplianceSettingsDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.UpdateComplianceSettingsAsync(userId, dto);
                return Ok(ApiResponse<ComplianceSettingsDto>.SuccessResponse(result, "Settings updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating compliance settings");
                return BadRequest(ApiResponse<ComplianceSettingsDto>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Compliance Checks

        /// <summary>
        /// Check if a contact is compliant for messaging on a specific channel
        /// </summary>
        [HttpGet("contacts/{contactId}/check")]
        public async Task<ActionResult<ApiResponse<ComplianceCheckResultDto>>> CheckCompliance(
            int contactId, [FromQuery] ConsentChannel channel, [FromQuery] int? campaignId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.CheckComplianceAsync(userId, contactId, channel, campaignId);
            return Ok(ApiResponse<ComplianceCheckResultDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Check if current time is within quiet hours
        /// </summary>
        [HttpGet("quiet-hours/check")]
        public async Task<ActionResult<ApiResponse<QuietHoursCheckDto>>> CheckQuietHours([FromQuery] DateTime? sendTime = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.CheckQuietHoursAsync(userId, sendTime);
            return Ok(ApiResponse<QuietHoursCheckDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Check if a contact is suppressed
        /// </summary>
        [HttpGet("check-suppression")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckSuppression(
            [FromQuery] string phoneOrEmail, [FromQuery] ConsentChannel? channel = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.IsContactSuppressedAsync(userId, phoneOrEmail, channel);
            return Ok(ApiResponse<bool>.SuccessResponse(result));
        }

        /// <summary>
        /// Filter a list of contact IDs to only include compliant contacts
        /// </summary>
        [HttpPost("filter-compliant")]
        public async Task<ActionResult<ApiResponse<List<int>>>> FilterCompliantContacts(
            [FromBody] List<int> contactIds, [FromQuery] ConsentChannel channel, [FromQuery] int? campaignId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.FilterCompliantContactsAsync(userId, contactIds, channel, campaignId);
            return Ok(ApiResponse<List<int>>.SuccessResponse(result, $"{result.Count} compliant contacts"));
        }

        #endregion

        #region Audit Logs

        /// <summary>
        /// Get compliance audit logs
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ComplianceAuditLogDto>>>> GetAuditLogs(
            [FromQuery] PagedRequest request,
            [FromQuery] ComplianceActionType? actionType = null,
            [FromQuery] ConsentChannel? channel = null,
            [FromQuery] int? contactId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _complianceService.GetAuditLogsAsync(userId, request, actionType, channel, contactId);
            return Ok(ApiResponse<PaginatedResult<ComplianceAuditLogDto>>.SuccessResponse(result));
        }

        #endregion

        #region Opt-In/Opt-Out Keywords

        /// <summary>
        /// Process an opt-out keyword
        /// </summary>
        [HttpPost("contacts/{contactId}/process-optout")]
        public async Task<ActionResult<ApiResponse<bool>>> ProcessOptOutKeyword(
            int contactId, [FromQuery] string keyword, [FromQuery] ConsentChannel channel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.ProcessOptOutKeywordAsync(userId, contactId, keyword, channel);
                if (result)
                {
                    var message = await _complianceService.GetOptOutConfirmationMessageAsync(userId);
                    return Ok(ApiResponse<bool>.SuccessResponse(result, message ?? "Opt-out processed successfully"));
                }
                return BadRequest(ApiResponse<bool>.ErrorResponse("Keyword not recognized as opt-out"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing opt-out keyword");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Process an opt-in keyword
        /// </summary>
        [HttpPost("contacts/{contactId}/process-optin")]
        public async Task<ActionResult<ApiResponse<bool>>> ProcessOptInKeyword(
            int contactId, [FromQuery] string keyword, [FromQuery] ConsentChannel channel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _complianceService.ProcessOptInKeywordAsync(userId, contactId, keyword, channel);
                if (result)
                {
                    var message = await _complianceService.GetOptInConfirmationMessageAsync(userId);
                    return Ok(ApiResponse<bool>.SuccessResponse(result, message ?? "Opt-in processed successfully"));
                }
                return BadRequest(ApiResponse<bool>.ErrorResponse("Keyword not recognized as opt-in"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing opt-in keyword");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        #endregion
    }
}
