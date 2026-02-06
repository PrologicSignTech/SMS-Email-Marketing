using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.User;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetUsers([FromQuery] PagedRequest request)
        {
            var users = await _userService.GetUsersAsync(request);
            return Ok(ApiResponse<PaginatedResult<UserDto>>.SuccessResponse(users));
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.UpdateUserAsync(userId, dto);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update profile"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Profile updated successfully"));
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<UserStatsDto>>> GetStats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var stats = await _userService.GetUserStatsAsync(userId);
            return Ok(ApiResponse<UserStatsDto>.SuccessResponse(stats));
        }

        [HttpPost("{userId}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(string userId)
        {
            var result = await _userService.DeactivateUserAsync(userId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to deactivate user"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "User deactivated successfully"));
        }

        [HttpPost("{userId}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(string userId)
        {
            var result = await _userService.ActivateUserAsync(userId);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to activate user"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "User activated successfully"));
        }
    }
}
