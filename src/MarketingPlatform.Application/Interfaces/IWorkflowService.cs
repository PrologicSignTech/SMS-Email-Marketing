using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Journey;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IWorkflowService
    {
        // Journey/Workflow Management (CRUD)
        
        /// <summary>
        /// Create a new journey/workflow
        /// </summary>
        Task<JourneyDto> CreateJourneyAsync(string userId, CreateJourneyDto dto);
        
        /// <summary>
        /// Get journey by ID
        /// </summary>
        Task<JourneyDto?> GetJourneyByIdAsync(string userId, int journeyId);
        
        /// <summary>
        /// Get all journeys for a user (paginated)
        /// </summary>
        Task<PaginatedResult<JourneyDto>> GetJourneysAsync(string userId, PagedRequest request);
        
        /// <summary>
        /// Update an existing journey/workflow
        /// </summary>
        Task<bool> UpdateJourneyAsync(string userId, int journeyId, UpdateJourneyDto dto);
        
        /// <summary>
        /// Delete a journey/workflow
        /// </summary>
        Task<bool> DeleteJourneyAsync(string userId, int journeyId);
        
        /// <summary>
        /// Duplicate an existing journey
        /// </summary>
        Task<JourneyDto> DuplicateJourneyAsync(string userId, int journeyId);
        
        /// <summary>
        /// Get journey statistics
        /// </summary>
        Task<JourneyStatsDto> GetJourneyStatsAsync(string userId, int journeyId);
        
        /// <summary>
        /// Get journey executions (paginated)
        /// </summary>
        Task<PaginatedResult<JourneyExecutionDto>> GetJourneyExecutionsAsync(string userId, int journeyId, PagedRequest request);
        
        // Journey Execution
        
        /// <summary>
        /// Execute a workflow for a specific contact
        /// </summary>
        Task ExecuteWorkflowAsync(int workflowId, int contactId);

        /// <summary>
        /// Process the next step in a workflow execution
        /// </summary>
        Task ProcessNextStepAsync(int executionId);

        /// <summary>
        /// Pause a running workflow execution
        /// </summary>
        Task PauseWorkflowExecutionAsync(int executionId);

        /// <summary>
        /// Resume a paused workflow execution
        /// </summary>
        Task ResumeWorkflowExecutionAsync(int executionId);

        /// <summary>
        /// Cancel a workflow execution
        /// </summary>
        Task CancelWorkflowExecutionAsync(int executionId);

        /// <summary>
        /// Get all active workflow executions for a contact
        /// </summary>
        Task<List<WorkflowExecution>> GetActiveExecutionsForContactAsync(int contactId);
    }
}
