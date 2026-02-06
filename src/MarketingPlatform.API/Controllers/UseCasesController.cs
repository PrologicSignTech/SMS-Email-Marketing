using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/usecases")]
    public class UseCasesController : ControllerBase
    {
        private readonly IUseCaseService _useCaseService;
        private readonly ILogger<UseCasesController> _logger;

        public UseCasesController(
            IUseCaseService useCaseService,
            ILogger<UseCasesController> logger)
        {
            _useCaseService = useCaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active use cases for landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UseCase>>>> GetUseCases()
        {
            try
            {
                var useCases = await _useCaseService.GetAllActiveAsync();
                var useCaseList = useCases.ToList();

                _logger.LogInformation("Retrieved {Count} use cases", useCaseList.Count);

                return Ok(ApiResponse<List<UseCase>>.SuccessResponse(useCaseList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving use cases");
                return BadRequest(ApiResponse<List<UseCase>>.ErrorResponse(
                    "Failed to retrieve use cases",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific use case by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UseCase>>> GetById(int id)
        {
            try
            {
                var useCase = await _useCaseService.GetByIdAsync(id);

                if (useCase == null)
                    return NotFound(ApiResponse<UseCase>.ErrorResponse("Use case not found"));

                return Ok(ApiResponse<UseCase>.SuccessResponse(useCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving use case {UseCaseId}", id);
                return BadRequest(ApiResponse<UseCase>.ErrorResponse(
                    "Failed to retrieve use case",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get use cases by industry
        /// </summary>
        [HttpGet("industry/{industry}")]
        public async Task<ActionResult<ApiResponse<List<UseCase>>>> GetByIndustry(string industry)
        {
            try
            {
                var useCases = await _useCaseService.GetByIndustryAsync(industry);
                var useCaseList = useCases.ToList();

                return Ok(ApiResponse<List<UseCase>>.SuccessResponse(useCaseList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving use cases for industry {Industry}", industry);
                return BadRequest(ApiResponse<List<UseCase>>.ErrorResponse(
                    "Failed to retrieve use cases",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new use case (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<UseCase>>> Create([FromBody] UseCase useCase)
        {
            try
            {
                var createdUseCase = await _useCaseService.CreateAsync(useCase);

                return Ok(ApiResponse<UseCase>.SuccessResponse(
                    createdUseCase,
                    "Use case created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating use case");
                return BadRequest(ApiResponse<UseCase>.ErrorResponse(
                    "Failed to create use case",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a use case (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<UseCase>>> Update(int id, [FromBody] UseCase useCase)
        {
            try
            {
                var updatedUseCase = await _useCaseService.UpdateAsync(id, useCase);

                return Ok(ApiResponse<UseCase>.SuccessResponse(
                    updatedUseCase,
                    "Use case updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<UseCase>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating use case {UseCaseId}", id);
                return BadRequest(ApiResponse<UseCase>.ErrorResponse(
                    "Failed to update use case",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a use case (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _useCaseService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Use case deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting use case {UseCaseId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete use case",
                    new List<string> { ex.Message }));
            }
        }
    }
}
