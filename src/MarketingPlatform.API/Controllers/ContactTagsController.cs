using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.ContactTag;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactTagsController : ControllerBase
    {
        private readonly IContactTagService _tagService;
        private readonly ILogger<ContactTagsController> _logger;

        public ContactTagsController(IContactTagService tagService, ILogger<ContactTagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ContactTagDto>>>> GetTags([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _tagService.GetAllAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<ContactTagDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContactTagDto>>> GetTag(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _tagService.GetByIdAsync(userId, id);
            if (result == null)
                return NotFound(ApiResponse<ContactTagDto>.ErrorResponse("Tag not found"));

            return Ok(ApiResponse<ContactTagDto>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContactTagDto>>> CreateTag([FromBody] CreateContactTagDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _tagService.CreateAsync(userId, dto);
                return Ok(ApiResponse<ContactTagDto>.SuccessResponse(result, "Tag created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return BadRequest(ApiResponse<ContactTagDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTag(int id, [FromBody] CreateContactTagDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _tagService.UpdateAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Tag not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Tag updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTag(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _tagService.DeleteAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Tag not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Tag deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("contacts/{contactId}/tags/{tagId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignTagToContact(int contactId, int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _tagService.AssignTagToContactAsync(userId, contactId, tagId);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to assign tag. Contact or tag not found, or tag already assigned."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Tag assigned to contact successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning tag to contact");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("contacts/{contactId}/tags/{tagId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveTagFromContact(int contactId, int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _tagService.RemoveTagFromContactAsync(userId, contactId, tagId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Contact, tag, or tag assignment not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Tag removed from contact successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag from contact");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("contacts/{contactId}")]
        public async Task<ActionResult<ApiResponse<List<ContactTagDto>>>> GetContactTags(int contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _tagService.GetContactTagsAsync(userId, contactId);
            return Ok(ApiResponse<List<ContactTagDto>>.SuccessResponse(result));
        }
    }
}
