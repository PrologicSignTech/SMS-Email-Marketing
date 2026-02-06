using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceRulesController : ControllerBase
    {
        private readonly IComplianceRuleService _complianceRuleService;
        private readonly ILogger<ComplianceRulesController> _logger;

        public ComplianceRulesController(
            IComplianceRuleService complianceRuleService,
            ILogger<ComplianceRulesController> logger)
        {
            _complianceRuleService = complianceRuleService;
            _logger = logger;
        }

        /// <summary>
        /// Get client IP address handling proxy scenarios
        /// </summary>
        private string? GetClientIpAddress()
        {
            // Check for X-Forwarded-For header (load balancers, proxies)
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim(); // First IP is the original client
                }
            }

            // Check for X-Real-IP header (nginx, other proxies)
            if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.ToString();
            }

            // Fallback to direct connection IP
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Get all compliance rules with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ComplianceRuleDto>>>> GetComplianceRules([FromQuery] PagedRequest request)
        {
            try
            {
                var rules = await _complianceRuleService.GetComplianceRulesAsync(request);
                return Ok(ApiResponse<PaginatedResult<ComplianceRuleDto>>.SuccessResponse(rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance rules");
                return StatusCode(500, ApiResponse<PaginatedResult<ComplianceRuleDto>>.ErrorResponse("Error retrieving compliance rules"));
            }
        }

        /// <summary>
        /// Get compliance rule by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ComplianceRuleDto>>> GetComplianceRuleById(int id)
        {
            try
            {
                var rule = await _complianceRuleService.GetComplianceRuleByIdAsync(id);
                if (rule == null)
                    return NotFound(ApiResponse<ComplianceRuleDto>.ErrorResponse("Compliance rule not found"));

                return Ok(ApiResponse<ComplianceRuleDto>.SuccessResponse(rule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<ComplianceRuleDto>.ErrorResponse("Error retrieving compliance rule"));
            }
        }

        /// <summary>
        /// Get all active compliance rules
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<ComplianceRuleDto>>>> GetActiveComplianceRules()
        {
            try
            {
                var rules = await _complianceRuleService.GetActiveComplianceRulesAsync();
                return Ok(ApiResponse<List<ComplianceRuleDto>>.SuccessResponse(rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active compliance rules");
                return StatusCode(500, ApiResponse<List<ComplianceRuleDto>>.ErrorResponse("Error retrieving active compliance rules"));
            }
        }

        /// <summary>
        /// Get compliance rules by type
        /// </summary>
        [HttpGet("type/{ruleType}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<ComplianceRuleDto>>>> GetComplianceRulesByType(ComplianceRuleType ruleType)
        {
            try
            {
                var rules = await _complianceRuleService.GetComplianceRulesByTypeAsync(ruleType);
                return Ok(ApiResponse<List<ComplianceRuleDto>>.SuccessResponse(rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance rules for type {Type}", ruleType);
                return StatusCode(500, ApiResponse<List<ComplianceRuleDto>>.ErrorResponse("Error retrieving compliance rules"));
            }
        }

        /// <summary>
        /// Get applicable rules for a region and/or service
        /// </summary>
        [HttpGet("applicable")]
        public async Task<ActionResult<ApiResponse<List<ComplianceRuleDto>>>> GetApplicableRules(
            [FromQuery] string? region = null,
            [FromQuery] string? service = null)
        {
            try
            {
                var rules = await _complianceRuleService.GetApplicableRulesAsync(region, service);
                return Ok(ApiResponse<List<ComplianceRuleDto>>.SuccessResponse(rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applicable compliance rules");
                return StatusCode(500, ApiResponse<List<ComplianceRuleDto>>.ErrorResponse("Error retrieving applicable rules"));
            }
        }

        /// <summary>
        /// Get audit trail for a compliance rule
        /// </summary>
        [HttpGet("{id}/audit")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<ComplianceRuleAuditDto>>>> GetComplianceRuleAuditTrail(int id)
        {
            try
            {
                var auditTrail = await _complianceRuleService.GetComplianceRuleAuditTrailAsync(id);
                return Ok(ApiResponse<List<ComplianceRuleAuditDto>>.SuccessResponse(auditTrail));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail for compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<List<ComplianceRuleAuditDto>>.ErrorResponse("Error retrieving audit trail"));
            }
        }

        /// <summary>
        /// Create a new compliance rule
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ComplianceRuleDto>>> CreateComplianceRule([FromBody] CreateComplianceRuleDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var rule = await _complianceRuleService.CreateComplianceRuleAsync(dto, userId);
                return CreatedAtAction(nameof(GetComplianceRuleById), new { id = rule.Id },
                    ApiResponse<ComplianceRuleDto>.SuccessResponse(rule, "Compliance rule created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compliance rule");
                return StatusCode(500, ApiResponse<ComplianceRuleDto>.ErrorResponse("Error creating compliance rule"));
            }
        }

        /// <summary>
        /// Update an existing compliance rule
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateComplianceRule(int id, [FromBody] UpdateComplianceRuleDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var ipAddress = GetClientIpAddress();

                var result = await _complianceRuleService.UpdateComplianceRuleAsync(id, dto, userId, ipAddress);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Compliance rule not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Compliance rule updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error updating compliance rule"));
            }
        }

        /// <summary>
        /// Activate a compliance rule
        /// </summary>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateComplianceRule(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var ipAddress = GetClientIpAddress();

                var result = await _complianceRuleService.ActivateComplianceRuleAsync(id, userId, ipAddress);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Compliance rule not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Compliance rule activated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error activating compliance rule"));
            }
        }

        /// <summary>
        /// Deactivate a compliance rule
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateComplianceRule(int id, [FromBody] string? reason = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var ipAddress = GetClientIpAddress();

                var result = await _complianceRuleService.DeactivateComplianceRuleAsync(id, userId, reason, ipAddress);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Compliance rule not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Compliance rule deactivated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deactivating compliance rule"));
            }
        }

        /// <summary>
        /// Delete a compliance rule
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteComplianceRule(int id, [FromQuery] string? reason = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var ipAddress = GetClientIpAddress();

                var result = await _complianceRuleService.DeleteComplianceRuleAsync(id, userId, reason, ipAddress);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Compliance rule not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Compliance rule deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting compliance rule {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting compliance rule"));
            }
        }
    }
}
