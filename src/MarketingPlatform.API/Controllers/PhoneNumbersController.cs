using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.PhoneNumber;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PhoneNumbersController : ControllerBase
    {
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly ILogger<PhoneNumbersController> _logger;

        public PhoneNumbersController(IPhoneNumberService phoneNumberService, ILogger<PhoneNumbersController> logger)
        {
            _phoneNumberService = phoneNumberService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<PhoneNumberDto>>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _phoneNumberService.GetAllAsync(request);
            return Ok(ApiResponse<PaginatedResult<PhoneNumberDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> GetById(int id)
        {
            var result = await _phoneNumberService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<PhoneNumberDto>.ErrorResponse("Phone number not found"));
            return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<PhoneNumberDto>>>> GetByUser(string userId)
        {
            var result = await _phoneNumberService.GetByUserAsync(userId);
            return Ok(ApiResponse<List<PhoneNumberDto>>.SuccessResponse(result));
        }

        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<PhoneNumberDto>>>> GetMyNumbers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _phoneNumberService.GetByUserAsync(userId);
            return Ok(ApiResponse<List<PhoneNumberDto>>.SuccessResponse(result));
        }

        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<PhoneNumberDto>>>> GetAvailable()
        {
            var result = await _phoneNumberService.GetAvailableAsync();
            return Ok(ApiResponse<List<PhoneNumberDto>>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> Create([FromBody] CreatePhoneNumberDto dto)
        {
            // Only SuperAdmin and Admin can add numbers
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
                return Forbid();

            try
            {
                var result = await _phoneNumberService.CreateAsync(dto);
                return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result, "Phone number added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating phone number");
                return BadRequest(ApiResponse<PhoneNumberDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> Update(int id, [FromBody] UpdatePhoneNumberDto dto)
        {
            try
            {
                var result = await _phoneNumberService.UpdateAsync(id, dto);
                return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result, "Phone number updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phone number");
                return BadRequest(ApiResponse<PhoneNumberDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/assign")]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> Assign(int id, [FromBody] AssignPhoneNumberDto dto)
        {
            // Only SuperAdmin and Admin can assign numbers
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
                return Forbid();

            try
            {
                var result = await _phoneNumberService.AssignAsync(id, dto.UserId);
                return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result, "Phone number assigned successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning phone number");
                return BadRequest(ApiResponse<PhoneNumberDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/unassign")]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> Unassign(int id)
        {
            // Only SuperAdmin and Admin can unassign numbers
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
                return Forbid();

            try
            {
                var result = await _phoneNumberService.UnassignAsync(id);
                return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result, "Phone number unassigned successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning phone number");
                return BadRequest(ApiResponse<PhoneNumberDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("purchase")]
        public async Task<ActionResult<ApiResponse<PhoneNumberDto>>> Purchase([FromBody] PurchasePhoneNumberDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _phoneNumberService.PurchaseAsync(userId, dto);
                return Ok(ApiResponse<PhoneNumberDto>.SuccessResponse(result, "Phone number purchased successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing phone number");
                return BadRequest(ApiResponse<PhoneNumberDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{id}/release")]
        public async Task<ActionResult<ApiResponse<bool>>> Release(int id)
        {
            try
            {
                var result = await _phoneNumberService.ReleaseAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Phone number not found"));
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Phone number released successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing phone number");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _phoneNumberService.DeleteAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Phone number not found"));
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Phone number deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting phone number");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
