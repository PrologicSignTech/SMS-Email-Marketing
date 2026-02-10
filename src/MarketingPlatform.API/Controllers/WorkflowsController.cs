using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Journey;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    /// <summary>
    /// Workflows API Controller
    /// Provides endpoints for managing automated workflows using real database
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowsController> _logger;

        public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Get all workflows (paginated)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<JourneyDto>>>> GetWorkflows([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _workflowService.GetJourneysAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<JourneyDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get workflow by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> GetWorkflow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _workflowService.GetJourneyByIdAsync(userId, id);
            if (result == null)
                return NotFound(ApiResponse<JourneyDto>.ErrorResponse("Workflow not found"));

            return Ok(ApiResponse<JourneyDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Create new workflow
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> CreateWorkflow([FromBody] CreateJourneyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.CreateJourneyAsync(userId, dto);
                return Ok(ApiResponse<JourneyDto>.SuccessResponse(result, "Workflow created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow");
                return BadRequest(ApiResponse<JourneyDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update workflow
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateWorkflow(int id, [FromBody] UpdateJourneyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.UpdateJourneyAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Workflow not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Workflow updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete workflow
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkflow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.DeleteJourneyAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Workflow not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Workflow deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Activate workflow (set IsActive = true)
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateWorkflow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var workflow = await _workflowService.GetJourneyByIdAsync(userId, id);
                if (workflow == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Workflow not found"));

                var updateDto = new UpdateJourneyDto
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    TriggerType = workflow.TriggerType,
                    TriggerCriteria = workflow.TriggerCriteria,
                    IsActive = true,
                    Nodes = workflow.Nodes.Select(n => new CreateJourneyNodeDto
                    {
                        StepOrder = n.StepOrder,
                        ActionType = n.ActionType,
                        ActionConfiguration = n.ActionConfiguration,
                        DelayMinutes = n.DelayMinutes,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY,
                        NodeLabel = n.NodeLabel,
                        BranchCondition = n.BranchCondition,
                        NextNodeOnTrue = n.NextNodeOnTrue,
                        NextNodeOnFalse = n.NextNodeOnFalse
                    }).ToList()
                };

                await _workflowService.UpdateJourneyAsync(userId, id, updateDto);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Workflow activated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating workflow {WorkflowId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Pause workflow (set IsActive = false)
        /// </summary>
        [HttpPost("{id}/pause")]
        public async Task<ActionResult<ApiResponse<bool>>> PauseWorkflow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var workflow = await _workflowService.GetJourneyByIdAsync(userId, id);
                if (workflow == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Workflow not found"));

                var updateDto = new UpdateJourneyDto
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    TriggerType = workflow.TriggerType,
                    TriggerCriteria = workflow.TriggerCriteria,
                    IsActive = false,
                    Nodes = workflow.Nodes.Select(n => new CreateJourneyNodeDto
                    {
                        StepOrder = n.StepOrder,
                        ActionType = n.ActionType,
                        ActionConfiguration = n.ActionConfiguration,
                        DelayMinutes = n.DelayMinutes,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY,
                        NodeLabel = n.NodeLabel,
                        BranchCondition = n.BranchCondition,
                        NextNodeOnTrue = n.NextNodeOnTrue,
                        NextNodeOnFalse = n.NextNodeOnFalse
                    }).ToList()
                };

                await _workflowService.UpdateJourneyAsync(userId, id, updateDto);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Workflow paused successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing workflow {WorkflowId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Duplicate workflow
        /// </summary>
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<ApiResponse<JourneyDto>>> DuplicateWorkflow(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.DuplicateJourneyAsync(userId, id);
                return Ok(ApiResponse<JourneyDto>.SuccessResponse(result, "Workflow duplicated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating workflow {WorkflowId}", id);
                return BadRequest(ApiResponse<JourneyDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get workflow statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ApiResponse<JourneyStatsDto>>> GetWorkflowStats(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _workflowService.GetJourneyStatsAsync(userId, id);
                return Ok(ApiResponse<JourneyStatsDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stats for workflow {WorkflowId}", id);
                return BadRequest(ApiResponse<JourneyStatsDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Execute workflow for a contact
        /// </summary>
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<ApiResponse<bool>>> ExecuteWorkflow(int id, [FromBody] ExecuteWorkflowRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _workflowService.ExecuteWorkflowAsync(id, request.ContactId);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Workflow execution started"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }

    public class ExecuteWorkflowRequest
    {
        public int ContactId { get; set; }
    }
}
