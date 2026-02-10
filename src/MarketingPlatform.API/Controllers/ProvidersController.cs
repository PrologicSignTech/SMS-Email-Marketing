using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Provider;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;
        private readonly ILogger<ProvidersController> _logger;

        public ProvidersController(IProviderService providerService, ILogger<ProvidersController> logger)
        {
            _providerService = providerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ProviderDto>>>> GetProviders([FromQuery] PagedRequest request)
        {
            var providers = await _providerService.GetProvidersAsync(request);
            return Ok(ApiResponse<PaginatedResult<ProviderDto>>.SuccessResponse(providers));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProviderDetailDto>>> GetProvider(int id)
        {
            var provider = await _providerService.GetProviderByIdAsync(id);
            if (provider == null)
                return NotFound(ApiResponse<ProviderDetailDto>.ErrorResponse("Provider not found"));

            return Ok(ApiResponse<ProviderDetailDto>.SuccessResponse(provider));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProviderDto>>> CreateProvider([FromBody] CreateProviderDto dto)
        {
            try
            {
                var provider = await _providerService.CreateProviderAsync(dto);
                return Ok(ApiResponse<ProviderDto>.SuccessResponse(provider, "Provider created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating provider");
                return BadRequest(ApiResponse<ProviderDto>.ErrorResponse("Failed to create provider", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProvider(int id, [FromBody] UpdateProviderDto dto)
        {
            var result = await _providerService.UpdateProviderAsync(id, dto);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update provider. Provider not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Provider updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProvider(int id)
        {
            var result = await _providerService.DeleteProviderAsync(id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete provider. Provider not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Provider deleted successfully"));
        }

        [HttpPost("{id}/test")]
        public async Task<ActionResult<ApiResponse<bool>>> TestProvider(int id)
        {
            var result = await _providerService.TestProviderAsync(id);
            if (!result)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to test provider. Provider not found."));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Provider connection test successful"));
        }

        [HttpGet("channeltypes")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetChannelTypes()
        {
            var channelTypes = await _providerService.GetChannelTypesAsync();
            return Ok(ApiResponse<List<string>>.SuccessResponse(channelTypes));
        }
    }
}
