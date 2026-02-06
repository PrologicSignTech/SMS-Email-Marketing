using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Pricing;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.API.Controllers
{
    /// <summary>
    /// Manages pricing models, channel pricing, region pricing, usage pricing, and tax configurations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(IPricingService pricingService, ILogger<PricingController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        // ======= Pricing Model Endpoints (12.5.1) =======

        /// <summary>
        /// Get all pricing models
        /// </summary>
        [HttpGet("models")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<PricingModelDto>>>> GetPricingModels([FromQuery] PagedRequest request)
        {
            var models = await _pricingService.GetPricingModelsAsync(request);
            return Ok(ApiResponse<PaginatedResult<PricingModelDto>>.SuccessResponse(models));
        }

        /// <summary>
        /// Get pricing model by ID
        /// </summary>
        [HttpGet("models/{id}")]
        public async Task<ActionResult<ApiResponse<PricingModelDto>>> GetPricingModel(int id)
        {
            var model = await _pricingService.GetPricingModelByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<PricingModelDto>.ErrorResponse("Pricing model not found"));

            return Ok(ApiResponse<PricingModelDto>.SuccessResponse(model));
        }

        /// <summary>
        /// Create a new pricing model
        /// </summary>
        [HttpPost("models")]
        public async Task<ActionResult<ApiResponse<PricingModelDto>>> CreatePricingModel([FromBody] CreatePricingModelDto dto)
        {
            try
            {
                var model = await _pricingService.CreatePricingModelAsync(dto);
                return Ok(ApiResponse<PricingModelDto>.SuccessResponse(model, "Pricing model created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pricing model");
                return BadRequest(ApiResponse<PricingModelDto>.ErrorResponse("Failed to create pricing model", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update pricing model
        /// </summary>
        [HttpPut("models/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePricingModel(int id, [FromBody] UpdatePricingModelDto dto)
        {
            var result = await _pricingService.UpdatePricingModelAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Pricing model not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Pricing model updated successfully"));
        }

        /// <summary>
        /// Delete pricing model
        /// </summary>
        [HttpDelete("models/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePricingModel(int id)
        {
            var result = await _pricingService.DeletePricingModelAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Pricing model not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Pricing model deleted successfully"));
        }

        // ======= Channel Pricing Endpoints (12.5.2) =======

        /// <summary>
        /// Get channel pricing by model ID
        /// </summary>
        [HttpGet("models/{modelId}/channels")]
        public async Task<ActionResult<ApiResponse<List<ChannelPricingDto>>>> GetChannelPricings(int modelId)
        {
            var pricings = await _pricingService.GetChannelPricingsByModelAsync(modelId);
            return Ok(ApiResponse<List<ChannelPricingDto>>.SuccessResponse(pricings));
        }

        /// <summary>
        /// Get channel pricing by ID
        /// </summary>
        [HttpGet("channels/{id}")]
        public async Task<ActionResult<ApiResponse<ChannelPricingDto>>> GetChannelPricing(int id)
        {
            var pricing = await _pricingService.GetChannelPricingByIdAsync(id);
            if (pricing == null)
                return NotFound(ApiResponse<ChannelPricingDto>.ErrorResponse("Channel pricing not found"));

            return Ok(ApiResponse<ChannelPricingDto>.SuccessResponse(pricing));
        }

        /// <summary>
        /// Create channel pricing
        /// </summary>
        [HttpPost("channels")]
        public async Task<ActionResult<ApiResponse<ChannelPricingDto>>> CreateChannelPricing([FromBody] CreateChannelPricingDto dto)
        {
            try
            {
                var pricing = await _pricingService.CreateChannelPricingAsync(dto);
                return Ok(ApiResponse<ChannelPricingDto>.SuccessResponse(pricing, "Channel pricing created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create channel pricing");
                return BadRequest(ApiResponse<ChannelPricingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating channel pricing");
                return BadRequest(ApiResponse<ChannelPricingDto>.ErrorResponse("Failed to create channel pricing", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update channel pricing
        /// </summary>
        [HttpPut("channels/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateChannelPricing(int id, [FromBody] UpdateChannelPricingDto dto)
        {
            var result = await _pricingService.UpdateChannelPricingAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Channel pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Channel pricing updated successfully"));
        }

        /// <summary>
        /// Delete channel pricing
        /// </summary>
        [HttpDelete("channels/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteChannelPricing(int id)
        {
            var result = await _pricingService.DeleteChannelPricingAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Channel pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Channel pricing deleted successfully"));
        }

        // ======= Region Pricing Endpoints (12.5.3) =======

        /// <summary>
        /// Get region pricing by model ID
        /// </summary>
        [HttpGet("models/{modelId}/regions")]
        public async Task<ActionResult<ApiResponse<List<RegionPricingDto>>>> GetRegionPricings(int modelId)
        {
            var pricings = await _pricingService.GetRegionPricingsByModelAsync(modelId);
            return Ok(ApiResponse<List<RegionPricingDto>>.SuccessResponse(pricings));
        }

        /// <summary>
        /// Get region pricing by ID
        /// </summary>
        [HttpGet("regions/{id}")]
        public async Task<ActionResult<ApiResponse<RegionPricingDto>>> GetRegionPricing(int id)
        {
            var pricing = await _pricingService.GetRegionPricingByIdAsync(id);
            if (pricing == null)
                return NotFound(ApiResponse<RegionPricingDto>.ErrorResponse("Region pricing not found"));

            return Ok(ApiResponse<RegionPricingDto>.SuccessResponse(pricing));
        }

        /// <summary>
        /// Create region pricing
        /// </summary>
        [HttpPost("regions")]
        public async Task<ActionResult<ApiResponse<RegionPricingDto>>> CreateRegionPricing([FromBody] CreateRegionPricingDto dto)
        {
            try
            {
                var pricing = await _pricingService.CreateRegionPricingAsync(dto);
                return Ok(ApiResponse<RegionPricingDto>.SuccessResponse(pricing, "Region pricing created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create region pricing");
                return BadRequest(ApiResponse<RegionPricingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating region pricing");
                return BadRequest(ApiResponse<RegionPricingDto>.ErrorResponse("Failed to create region pricing", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update region pricing
        /// </summary>
        [HttpPut("regions/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateRegionPricing(int id, [FromBody] UpdateRegionPricingDto dto)
        {
            var result = await _pricingService.UpdateRegionPricingAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Region pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Region pricing updated successfully"));
        }

        /// <summary>
        /// Delete region pricing
        /// </summary>
        [HttpDelete("regions/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRegionPricing(int id)
        {
            var result = await _pricingService.DeleteRegionPricingAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Region pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Region pricing deleted successfully"));
        }

        // ======= Usage Pricing Endpoints (12.5.4) =======

        /// <summary>
        /// Get usage pricing by model ID
        /// </summary>
        [HttpGet("models/{modelId}/usage")]
        public async Task<ActionResult<ApiResponse<List<UsagePricingDto>>>> GetUsagePricings(int modelId)
        {
            var pricings = await _pricingService.GetUsagePricingsByModelAsync(modelId);
            return Ok(ApiResponse<List<UsagePricingDto>>.SuccessResponse(pricings));
        }

        /// <summary>
        /// Get usage pricing by ID
        /// </summary>
        [HttpGet("usage/{id}")]
        public async Task<ActionResult<ApiResponse<UsagePricingDto>>> GetUsagePricing(int id)
        {
            var pricing = await _pricingService.GetUsagePricingByIdAsync(id);
            if (pricing == null)
                return NotFound(ApiResponse<UsagePricingDto>.ErrorResponse("Usage pricing not found"));

            return Ok(ApiResponse<UsagePricingDto>.SuccessResponse(pricing));
        }

        /// <summary>
        /// Create usage pricing
        /// </summary>
        [HttpPost("usage")]
        public async Task<ActionResult<ApiResponse<UsagePricingDto>>> CreateUsagePricing([FromBody] CreateUsagePricingDto dto)
        {
            try
            {
                var pricing = await _pricingService.CreateUsagePricingAsync(dto);
                return Ok(ApiResponse<UsagePricingDto>.SuccessResponse(pricing, "Usage pricing created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create usage pricing");
                return BadRequest(ApiResponse<UsagePricingDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating usage pricing");
                return BadRequest(ApiResponse<UsagePricingDto>.ErrorResponse("Failed to create usage pricing", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update usage pricing
        /// </summary>
        [HttpPut("usage/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUsagePricing(int id, [FromBody] UpdateUsagePricingDto dto)
        {
            var result = await _pricingService.UpdateUsagePricingAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Usage pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Usage pricing updated successfully"));
        }

        /// <summary>
        /// Delete usage pricing
        /// </summary>
        [HttpDelete("usage/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUsagePricing(int id)
        {
            var result = await _pricingService.DeleteUsagePricingAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Usage pricing not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Usage pricing deleted successfully"));
        }

        // ======= Tax Configuration Endpoints (12.5.5) =======

        /// <summary>
        /// Get all tax configurations
        /// </summary>
        [HttpGet("taxes")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TaxConfigurationDto>>>> GetTaxConfigurations([FromQuery] PagedRequest request)
        {
            var configs = await _pricingService.GetTaxConfigurationsAsync(request);
            return Ok(ApiResponse<PaginatedResult<TaxConfigurationDto>>.SuccessResponse(configs));
        }

        /// <summary>
        /// Get tax configuration by ID
        /// </summary>
        [HttpGet("taxes/{id}")]
        public async Task<ActionResult<ApiResponse<TaxConfigurationDto>>> GetTaxConfiguration(int id)
        {
            var config = await _pricingService.GetTaxConfigurationByIdAsync(id);
            if (config == null)
                return NotFound(ApiResponse<TaxConfigurationDto>.ErrorResponse("Tax configuration not found"));

            return Ok(ApiResponse<TaxConfigurationDto>.SuccessResponse(config));
        }

        /// <summary>
        /// Create tax configuration
        /// </summary>
        [HttpPost("taxes")]
        public async Task<ActionResult<ApiResponse<TaxConfigurationDto>>> CreateTaxConfiguration([FromBody] CreateTaxConfigurationDto dto)
        {
            try
            {
                var config = await _pricingService.CreateTaxConfigurationAsync(dto);
                return Ok(ApiResponse<TaxConfigurationDto>.SuccessResponse(config, "Tax configuration created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax configuration");
                return BadRequest(ApiResponse<TaxConfigurationDto>.ErrorResponse("Failed to create tax configuration", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update tax configuration
        /// </summary>
        [HttpPut("taxes/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTaxConfiguration(int id, [FromBody] UpdateTaxConfigurationDto dto)
        {
            var result = await _pricingService.UpdateTaxConfigurationAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Tax configuration not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Tax configuration updated successfully"));
        }

        /// <summary>
        /// Delete tax configuration
        /// </summary>
        [HttpDelete("taxes/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTaxConfiguration(int id)
        {
            var result = await _pricingService.DeleteTaxConfigurationAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Tax configuration not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Tax configuration deleted successfully"));
        }
    }
}
