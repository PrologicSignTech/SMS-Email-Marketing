using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/landingstats")]
    public class LandingStatsController : ControllerBase
    {
        private readonly ILandingStatService _landingStatService;
        private readonly ILogger<LandingStatsController> _logger;

        public LandingStatsController(
            ILandingStatService landingStatService,
            ILogger<LandingStatsController> logger)
        {
            _landingStatService = landingStatService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active landing stats for display on landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<LandingStat>>>> GetLandingStats()
        {
            try
            {
                var stats = await _landingStatService.GetAllActiveAsync();
                var statList = stats.ToList();

                _logger.LogInformation("Retrieved {Count} landing stats", statList.Count);

                return Ok(ApiResponse<List<LandingStat>>.SuccessResponse(statList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving landing stats");
                return BadRequest(ApiResponse<List<LandingStat>>.ErrorResponse(
                    "Failed to retrieve landing stats",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific landing stat by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LandingStat>>> GetById(int id)
        {
            try
            {
                var stat = await _landingStatService.GetByIdAsync(id);

                if (stat == null)
                    return NotFound(ApiResponse<LandingStat>.ErrorResponse("Stat not found"));

                return Ok(ApiResponse<LandingStat>.SuccessResponse(stat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stat {StatId}", id);
                return BadRequest(ApiResponse<LandingStat>.ErrorResponse(
                    "Failed to retrieve stat",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new landing stat (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingStat>>> Create([FromBody] LandingStat stat)
        {
            try
            {
                var createdStat = await _landingStatService.CreateAsync(stat);

                return Ok(ApiResponse<LandingStat>.SuccessResponse(
                    createdStat,
                    "Stat created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating landing stat");
                return BadRequest(ApiResponse<LandingStat>.ErrorResponse(
                    "Failed to create stat",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a landing stat (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingStat>>> Update(int id, [FromBody] LandingStat stat)
        {
            try
            {
                var updatedStat = await _landingStatService.UpdateAsync(id, stat);

                return Ok(ApiResponse<LandingStat>.SuccessResponse(
                    updatedStat,
                    "Stat updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<LandingStat>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stat {StatId}", id);
                return BadRequest(ApiResponse<LandingStat>.ErrorResponse(
                    "Failed to update stat",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a landing stat (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _landingStatService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Stat deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stat {StatId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete stat",
                    new List<string> { ex.Message }));
            }
        }
    }
}
