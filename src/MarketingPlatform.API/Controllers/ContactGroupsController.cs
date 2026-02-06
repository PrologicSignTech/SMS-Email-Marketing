using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.DTOs.ContactGroup;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactGroupsController : ControllerBase
    {
        private readonly IContactGroupService _groupService;
        private readonly IDynamicGroupEvaluationService _dynamicGroupService;
        private readonly ILogger<ContactGroupsController> _logger;

        public ContactGroupsController(
            IContactGroupService groupService, 
            IDynamicGroupEvaluationService dynamicGroupService,
            ILogger<ContactGroupsController> logger)
        {
            _groupService = groupService;
            _dynamicGroupService = dynamicGroupService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ContactGroupDto>>>> GetGroups([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var groups = await _groupService.GetGroupsAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<ContactGroupDto>>.SuccessResponse(groups));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContactGroupDto>>> GetGroup(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var group = await _groupService.GetGroupByIdAsync(userId, id);
            if (group == null)
                return NotFound(ApiResponse<ContactGroupDto>.ErrorResponse("Group not found"));

            return Ok(ApiResponse<ContactGroupDto>.SuccessResponse(group));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContactGroupDto>>> CreateGroup([FromBody] CreateContactGroupDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var group = await _groupService.CreateGroupAsync(userId, dto);
                return Ok(ApiResponse<ContactGroupDto>.SuccessResponse(group, "Group created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return BadRequest(ApiResponse<ContactGroupDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateGroup(int id, [FromBody] CreateContactGroupDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _groupService.UpdateGroupAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Group not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Group updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteGroup(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _groupService.DeleteGroupAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Group not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Group deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{groupId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AddContactToGroup(int groupId, int contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _groupService.AddContactToGroupAsync(userId, groupId, contactId);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to add contact to group. Group or contact not found, or contact already in group."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Contact added to group successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact to group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{groupId}/contacts/{contactId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveContactFromGroup(int groupId, int contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _groupService.RemoveContactFromGroupAsync(userId, groupId, contactId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Group or contact membership not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Contact removed from group successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing contact from group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}/contacts")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ContactDto>>>> GetGroupContacts(int id, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var contacts = await _groupService.GetGroupContactsAsync(userId, id, request);
            return Ok(ApiResponse<PaginatedResult<ContactDto>>.SuccessResponse(contacts));
        }

        [HttpPost("{id}/refresh")]
        public async Task<ActionResult<ApiResponse<bool>>> RefreshDynamicGroup(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _dynamicGroupService.UpdateDynamicGroupMembershipsAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Dynamic group not found or is not a dynamic group"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Dynamic group refreshed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dynamic group");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("refresh-all")]
        public async Task<ActionResult<ApiResponse<bool>>> RefreshAllDynamicGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _dynamicGroupService.UpdateAllDynamicGroupsAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "All dynamic groups refreshed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all dynamic groups");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
