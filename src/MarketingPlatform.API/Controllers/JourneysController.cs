using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Journey;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    /// <summary>
    /// Contact Journey Mapping / Workflow Designer API
    /// Enables visual workflow/journey creation, editing, and monitoring
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JourneysController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<JourneysController> _logger;

        public JourneysController(IWorkflowService workflowService, ILogger<JourneysController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Get all journeys/workflows for the current user (paginated)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<JourneyDto>>>> GetJourneys([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var journeys = await _workflowService.GetJourneysAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<JourneyDto>>.SuccessResponse(journeys));
        }

        /// <summary>
        /// Get a specific journey/workflow by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> GetJourney(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var journey = await _workflowService.GetJourneyByIdAsync(userId, id);
            if (journey == null)
                return NotFound(ApiResponse<JourneyDto>.ErrorResponse("Journey not found"));

            return Ok(ApiResponse<JourneyDto>.SuccessResponse(journey));
        }

        /// <summary>
        /// Create a new journey/workflow
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> CreateJourney([FromBody] CreateJourneyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var journey = await _workflowService.CreateJourneyAsync(userId, dto);
                return Ok(ApiResponse<JourneyDto>.SuccessResponse(journey, "Journey created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating journey");
                return BadRequest(ApiResponse<JourneyDto>.ErrorResponse("Failed to create journey", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update an existing journey/workflow
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateJourney(int id, [FromBody] UpdateJourneyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.UpdateJourneyAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Journey not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating journey {JourneyId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update journey", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a journey/workflow
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteJourney(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.DeleteJourneyAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Journey not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting journey {JourneyId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete journey", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Duplicate an existing journey/workflow
        /// </summary>
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> DuplicateJourney(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var journey = await _workflowService.DuplicateJourneyAsync(userId, id);
                return Ok(ApiResponse<JourneyDto>.SuccessResponse(journey, "Journey duplicated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<JourneyDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating journey {JourneyId}", id);
                return BadRequest(ApiResponse<JourneyDto>.ErrorResponse("Failed to duplicate journey", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get journey statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<JourneyStatsDto>>> GetJourneyStats(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var stats = await _workflowService.GetJourneyStatsAsync(userId, id);
                return Ok(ApiResponse<JourneyStatsDto>.SuccessResponse(stats));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<JourneyStatsDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting journey stats {JourneyId}", id);
                return BadRequest(ApiResponse<JourneyStatsDto>.ErrorResponse("Failed to get journey stats", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get journey executions (paginated)
        /// </summary>
        [HttpGet("{id}/executions")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<JourneyExecutionDto>>>> GetJourneyExecutions(
            int id, [FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var executions = await _workflowService.GetJourneyExecutionsAsync(userId, id, request);
                return Ok(ApiResponse<PaginatedResult<JourneyExecutionDto>>.SuccessResponse(executions));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<PaginatedResult<JourneyExecutionDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting journey executions {JourneyId}", id);
                return BadRequest(ApiResponse<PaginatedResult<JourneyExecutionDto>>.ErrorResponse(
                    "Failed to get journey executions", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Execute a journey for a specific contact
        /// </summary>
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<ApiResponse<bool>>> ExecuteJourney(int id, [FromBody] int contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Verify journey belongs to user
                var journey = await _workflowService.GetJourneyByIdAsync(userId, id);
                if (journey == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Journey not found"));

                await _workflowService.ExecuteWorkflowAsync(id, contactId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey execution started"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing journey {JourneyId} for contact {ContactId}", id, contactId);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to execute journey", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Pause a running journey execution
        /// </summary>
        [HttpPost("executions/{executionId}/pause")]
        public async Task<ActionResult<ApiResponse<bool>>> PauseExecution(int executionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _workflowService.PauseWorkflowExecutionAsync(executionId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey execution paused"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing execution {ExecutionId}", executionId);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to pause execution", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Resume a paused journey execution
        /// </summary>
        [HttpPost("executions/{executionId}/resume")]
        public async Task<ActionResult<ApiResponse<bool>>> ResumeExecution(int executionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _workflowService.ResumeWorkflowExecutionAsync(executionId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey execution resumed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming execution {ExecutionId}", executionId);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to resume execution", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Cancel a journey execution
        /// </summary>
        [HttpPost("executions/{executionId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelExecution(int executionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _workflowService.CancelWorkflowExecutionAsync(executionId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Journey execution cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel execution", new List<string> { ex.Message }));
            }
        }
    }
}
