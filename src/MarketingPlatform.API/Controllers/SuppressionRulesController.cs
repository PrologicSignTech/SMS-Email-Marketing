using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionRule;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SuppressionRulesController : ControllerBase
    {
        private readonly ISuppressionRuleService _ruleService;
        private readonly ILogger<SuppressionRulesController> _logger;

        public SuppressionRulesController(ISuppressionRuleService ruleService, ILogger<SuppressionRulesController> logger)
        {
            _ruleService = ruleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<SuppressionRuleDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _ruleService.GetAllAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<SuppressionRuleDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SuppressionRuleDto>>> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _ruleService.GetByIdAsync(userId, id);
            if (result == null)
                return NotFound(ApiResponse<SuppressionRuleDto>.ErrorResponse("Rule not found"));
            return Ok(ApiResponse<SuppressionRuleDto>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<SuppressionRuleDto>>>> GetActive()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _ruleService.GetActiveRulesAsync(userId);
            return Ok(ApiResponse<List<SuppressionRuleDto>>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SuppressionRuleDto>>> Create([FromBody] CreateSuppressionRuleDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _ruleService.CreateAsync(userId, dto);
                return Ok(ApiResponse<SuppressionRuleDto>.SuccessResponse(result, "Rule created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating suppression rule");
                return BadRequest(ApiResponse<SuppressionRuleDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SuppressionRuleDto>>> Update(int id, [FromBody] UpdateSuppressionRuleDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _ruleService.UpdateAsync(userId, id, dto);
                return Ok(ApiResponse<SuppressionRuleDto>.SuccessResponse(result, "Rule updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating suppression rule");
                return BadRequest(ApiResponse<SuppressionRuleDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/toggle")]
        public async Task<ActionResult<ApiResponse<bool>>> Toggle(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _ruleService.ToggleAsync(userId, id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Rule not found"));
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Rule toggled successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _ruleService.DeleteAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Rule not found"));
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Rule deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting suppression rule");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("seed-defaults")]
        public async Task<ActionResult<ApiResponse<bool>>> SeedDefaults()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _ruleService.SeedDefaultRulesAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Default rules seeded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default rules");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
