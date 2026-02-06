using MarketingPlatform.API.Authorization;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Role;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(roles, "Roles retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, ApiResponse<IEnumerable<RoleDto>>.ErrorResponse("Failed to retrieve roles"));
            }
        }

        [HttpGet("active")]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetActiveRoles()
        {
            try
            {
                var roles = await _roleService.GetActiveRolesAsync();
                return Ok(ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(roles, "Active roles retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active roles");
                return StatusCode(500, ApiResponse<IEnumerable<RoleDto>>.ErrorResponse("Failed to retrieve active roles"));
            }
        }

        [HttpGet("{id}")]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found"));
                }
                return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Role retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                return StatusCode(500, ApiResponse<RoleDto>.ErrorResponse("Failed to retrieve role"));
            }
        }

        [HttpGet("name/{name}")]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleByName(string name)
        {
            try
            {
                var role = await _roleService.GetRoleByNameAsync(name);
                if (role == null)
                {
                    return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found"));
                }
                return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Role retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleName}", name);
                return StatusCode(500, ApiResponse<RoleDto>.ErrorResponse("Failed to retrieve role"));
            }
        }

        [HttpPost]
        [RequirePermission(Permission.ManageRoles)]
        public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var role = await _roleService.CreateRoleAsync(createRoleDto, userId);
                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, 
                    ApiResponse<RoleDto>.SuccessResponse(role, "Role created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, ApiResponse<RoleDto>.ErrorResponse("Failed to create role"));
            }
        }

        [HttpPut("{id}")]
        [RequirePermission(Permission.ManageRoles)]
        public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(id, updateRoleDto);
                return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Role updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(500, ApiResponse<RoleDto>.ErrorResponse("Failed to update role"));
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permission.ManageRoles)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(int id)
        {
            try
            {
                await _roleService.DeleteRoleAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Role deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete role"));
            }
        }

        [HttpGet("{id}/users")]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserRoleDto>>>> GetUsersInRole(int id)
        {
            try
            {
                var users = await _roleService.GetUsersInRoleAsync(id);
                return Ok(ApiResponse<IEnumerable<UserRoleDto>>.SuccessResponse(users, "Users retrieved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserRoleDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for role {RoleId}", id);
                return StatusCode(500, ApiResponse<IEnumerable<UserRoleDto>>.ErrorResponse("Failed to retrieve users"));
            }
        }

        [HttpPost("assign")]
        [RequirePermission(Permission.ManageRoles)]
        public async Task<ActionResult<ApiResponse<bool>>> AssignRoleToUser([FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                var assignedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                await _roleService.AssignRoleToUserAsync(assignRoleDto.UserId, assignRoleDto.RoleId, assignedBy);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Role assigned successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to assign role"));
            }
        }

        [HttpDelete("remove")]
        [RequirePermission(Permission.ManageRoles)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveRoleFromUser([FromQuery] string userId, [FromQuery] int roleId)
        {
            try
            {
                await _roleService.RemoveRoleFromUserAsync(userId, roleId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Role removed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to remove role"));
            }
        }

        [HttpGet("permissions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetAllPermissions()
        {
            try
            {
                var permissions = Enum.GetValues(typeof(Permission))
                    .Cast<Permission>()
                    .Where(p => p != Permission.All)
                    .Select(p => p.ToString())
                    .ToList();
                
                return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(permissions, "Permissions retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, ApiResponse<IEnumerable<string>>.ErrorResponse("Failed to retrieve permissions"));
            }
        }

        [HttpGet("user/{userId}/permissions")]
        [RequirePermission(Permission.ViewRoles)]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetUserPermissions(string userId)
        {
            try
            {
                var permissions = await _roleService.GetUserPermissionsAsync(userId);
                return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(permissions, "User permissions retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user permissions");
                return StatusCode(500, ApiResponse<IEnumerable<string>>.ErrorResponse("Failed to retrieve user permissions"));
            }
        }
    }
}
