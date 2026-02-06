using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Template;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all templates (paginated)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TemplateDto>>>> GetTemplates([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var templates = await _templateService.GetTemplatesAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<TemplateDto>>.SuccessResponse(templates));
        }

        /// <summary>
        /// Get single template by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TemplateDto>>> GetTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var template = await _templateService.GetTemplateByIdAsync(userId, id);
            if (template == null)
                return NotFound(ApiResponse<TemplateDto>.ErrorResponse("Template not found"));

            return Ok(ApiResponse<TemplateDto>.SuccessResponse(template));
        }

        /// <summary>
        /// Get templates by channel (SMS, MMS, Email)
        /// </summary>
        [HttpGet("channel/{channel}")]
        public async Task<ActionResult<ApiResponse<List<TemplateDto>>>> GetTemplatesByChannel(ChannelType channel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var templates = await _templateService.GetTemplatesByChannelAsync(userId, channel);
            return Ok(ApiResponse<List<TemplateDto>>.SuccessResponse(templates));
        }

        /// <summary>
        /// Get templates by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<List<TemplateDto>>>> GetTemplatesByCategory(TemplateCategory category)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var templates = await _templateService.GetTemplatesByCategoryAsync(userId, category);
            return Ok(ApiResponse<List<TemplateDto>>.SuccessResponse(templates));
        }

        /// <summary>
        /// Create new template
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TemplateDto>>> CreateTemplate([FromBody] CreateTemplateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var template = await _templateService.CreateTemplateAsync(userId, dto);
                return Ok(ApiResponse<TemplateDto>.SuccessResponse(template, "Template created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                return BadRequest(ApiResponse<TemplateDto>.ErrorResponse("Failed to create template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update existing template
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTemplate(int id, [FromBody] UpdateTemplateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.UpdateTemplateAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete template
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.DeleteTemplateAsync(userId, id);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete template - it may be in use or not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Duplicate template
        /// </summary>
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<ApiResponse<bool>>> DuplicateTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.DuplicateTemplateAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template duplicated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to duplicate template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Set template as default for its channel and category
        /// </summary>
        [HttpPost("{id}/set-default")]
        public async Task<ActionResult<ApiResponse<bool>>> SetDefaultTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.SetDefaultTemplateAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template set as default successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to set default template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Activate template
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.ActivateTemplateAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template activated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to activate template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Deactivate template
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateTemplate(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _templateService.DeactivateTemplateAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Template not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Template deactivated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating template");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to deactivate template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Preview template with variable substitution
        /// </summary>
        [HttpPost("preview")]
        public async Task<ActionResult<ApiResponse<TemplatePreviewDto>>> PreviewTemplate([FromBody] TemplatePreviewRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var preview = await _templateService.PreviewTemplateAsync(userId, request);
                return Ok(ApiResponse<TemplatePreviewDto>.SuccessResponse(preview));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing template");
                return BadRequest(ApiResponse<TemplatePreviewDto>.ErrorResponse("Failed to preview template", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Extract variables from content
        /// </summary>
        [HttpPost("extract-variables")]
        public async Task<ActionResult<ApiResponse<List<string>>>> ExtractVariables([FromBody] string content)
        {
            try
            {
                var variables = await _templateService.ExtractVariablesFromContentAsync(content);
                return Ok(ApiResponse<List<string>>.SuccessResponse(variables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting variables");
                return BadRequest(ApiResponse<List<string>>.ErrorResponse("Failed to extract variables", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get template usage statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<TemplateUsageStatsDto>>> GetTemplateStats(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var stats = await _templateService.GetTemplateUsageStatsAsync(userId, id);
                return Ok(ApiResponse<TemplateUsageStatsDto>.SuccessResponse(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template stats");
                return BadRequest(ApiResponse<TemplateUsageStatsDto>.ErrorResponse("Failed to get template stats", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Calculate character count for content
        /// </summary>
        [HttpPost("calculate-character-count")]
        public async Task<ActionResult<ApiResponse<CharacterCountDto>>> CalculateCharacterCount([FromBody] CalculateCharacterCountRequestDto request)
        {
            try
            {
                var result = await _templateService.CalculateCharacterCountAsync(request.Content, request.Channel, request.IsSubject);
                return Ok(ApiResponse<CharacterCountDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating character count");
                return BadRequest(ApiResponse<CharacterCountDto>.ErrorResponse("Failed to calculate character count", new List<string> { ex.Message }));
            }
        }
    }
}
