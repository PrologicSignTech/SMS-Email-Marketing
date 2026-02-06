using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/landingfeatures")]
    public class LandingFeaturesController : ControllerBase
    {
        private readonly ILandingFeatureService _landingFeatureService;
        private readonly ILogger<LandingFeaturesController> _logger;

        public LandingFeaturesController(
            ILandingFeatureService landingFeatureService,
            ILogger<LandingFeaturesController> logger)
        {
            _landingFeatureService = landingFeatureService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active landing features for display on landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<LandingFeature>>>> GetLandingFeatures()
        {
            try
            {
                var features = await _landingFeatureService.GetAllActiveAsync();
                var featureList = features.ToList();

                _logger.LogInformation("Retrieved {Count} landing features", featureList.Count);

                return Ok(ApiResponse<List<LandingFeature>>.SuccessResponse(featureList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving landing features");
                return BadRequest(ApiResponse<List<LandingFeature>>.ErrorResponse(
                    "Failed to retrieve landing features",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific landing feature by ID (Public access for detail page)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LandingFeature>>> GetById(int id)
        {
            try
            {
                var feature = await _landingFeatureService.GetByIdAsync(id);

                if (feature == null)
                    return NotFound(ApiResponse<LandingFeature>.ErrorResponse("Feature not found"));

                return Ok(ApiResponse<LandingFeature>.SuccessResponse(feature));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feature {FeatureId}", id);
                return BadRequest(ApiResponse<LandingFeature>.ErrorResponse(
                    "Failed to retrieve feature",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new landing feature (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingFeature>>> Create([FromBody] LandingFeature feature)
        {
            try
            {
                var createdFeature = await _landingFeatureService.CreateAsync(feature);

                return Ok(ApiResponse<LandingFeature>.SuccessResponse(
                    createdFeature,
                    "Feature created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating landing feature");
                return BadRequest(ApiResponse<LandingFeature>.ErrorResponse(
                    "Failed to create feature",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a landing feature (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingFeature>>> Update(int id, [FromBody] LandingFeature feature)
        {
            try
            {
                var updatedFeature = await _landingFeatureService.UpdateAsync(id, feature);

                return Ok(ApiResponse<LandingFeature>.SuccessResponse(
                    updatedFeature,
                    "Feature updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<LandingFeature>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feature {FeatureId}", id);
                return BadRequest(ApiResponse<LandingFeature>.ErrorResponse(
                    "Failed to update feature",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a landing feature (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _landingFeatureService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Feature deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature {FeatureId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete feature",
                    new List<string> { ex.Message }));
            }
        }
    }
}
