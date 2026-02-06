using Hangfire;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Journey;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IRepository<WorkflowStep> _stepRepository;
        private readonly IRepository<WorkflowExecution> _executionRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IRepository<ContactTagAssignment> _tagAssignmentRepository;
        private readonly IMessageService _messageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(
            IRepository<Workflow> workflowRepository,
            IRepository<WorkflowStep> stepRepository,
            IRepository<WorkflowExecution> executionRepository,
            IRepository<Contact> contactRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IRepository<ContactTagAssignment> tagAssignmentRepository,
            IMessageService messageService,
            IUnitOfWork unitOfWork,
            ILogger<WorkflowService> logger)
        {
            _workflowRepository = workflowRepository;
            _stepRepository = stepRepository;
            _executionRepository = executionRepository;
            _contactRepository = contactRepository;
            _groupMemberRepository = groupMemberRepository;
            _tagAssignmentRepository = tagAssignmentRepository;
            _messageService = messageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Journey/Workflow Management (CRUD)
        
        public async Task<JourneyDto> CreateJourneyAsync(string userId, CreateJourneyDto dto)
        {
            var workflow = new Workflow
            {
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                TriggerType = dto.TriggerType,
                TriggerCriteria = dto.TriggerCriteria,
                IsActive = dto.IsActive
            };

            await _workflowRepository.AddAsync(workflow);
            await _unitOfWork.SaveChangesAsync();

            // Create workflow steps
            foreach (var nodeDto in dto.Nodes)
            {
                var step = new WorkflowStep
                {
                    WorkflowId = workflow.Id,
                    StepOrder = nodeDto.StepOrder,
                    ActionType = nodeDto.ActionType,
                    ActionConfiguration = nodeDto.ActionConfiguration,
                    DelayMinutes = nodeDto.DelayMinutes,
                    PositionX = nodeDto.PositionX,
                    PositionY = nodeDto.PositionY,
                    NodeLabel = nodeDto.NodeLabel,
                    BranchCondition = nodeDto.BranchCondition,
                    NextNodeOnTrue = nodeDto.NextNodeOnTrue,
                    NextNodeOnFalse = nodeDto.NextNodeOnFalse
                };

                await _stepRepository.AddAsync(step);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created journey {JourneyId} for user {UserId}", workflow.Id, userId);

            return await GetJourneyByIdAsync(userId, workflow.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created journey");
        }

        public async Task<JourneyDto?> GetJourneyByIdAsync(string userId, int journeyId)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => 
                w.Id == journeyId && w.UserId == userId && !w.IsDeleted);

            if (workflow == null)
                return null;

            var steps = await _stepRepository.FindAsync(s => 
                s.WorkflowId == journeyId && !s.IsDeleted);

            var executions = await _executionRepository.FindAsync(e => 
                e.WorkflowId == journeyId && !e.IsDeleted);

            return new JourneyDto
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                TriggerType = workflow.TriggerType,
                TriggerCriteria = workflow.TriggerCriteria,
                IsActive = workflow.IsActive,
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt,
                Nodes = steps.Select(s => new JourneyNodeDto
                {
                    Id = s.Id,
                    StepOrder = s.StepOrder,
                    ActionType = s.ActionType,
                    ActionConfiguration = s.ActionConfiguration,
                    DelayMinutes = s.DelayMinutes,
                    PositionX = s.PositionX,
                    PositionY = s.PositionY,
                    NodeLabel = s.NodeLabel,
                    BranchCondition = s.BranchCondition,
                    NextNodeOnTrue = s.NextNodeOnTrue,
                    NextNodeOnFalse = s.NextNodeOnFalse
                }).ToList(),
                TotalExecutions = executions.Count(),
                ActiveExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Running),
                CompletedExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Completed),
                FailedExecutions = executions.Count(e => e.Status == WorkflowExecutionStatus.Failed)
            };
        }

        public async Task<PaginatedResult<JourneyDto>> GetJourneysAsync(string userId, PagedRequest request)
        {
            var query = await _workflowRepository.FindAsync(w => 
                w.UserId == userId && !w.IsDeleted);

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(w => 
                    w.Name.Contains(request.SearchTerm) || 
                    (w.Description != null && w.Description.Contains(request.SearchTerm)));
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var journeyDtos = new List<JourneyDto>();
            foreach (var workflow in items)
            {
                var dto = await GetJourneyByIdAsync(userId, workflow.Id);
                if (dto != null)
                    journeyDtos.Add(dto);
            }

            return new PaginatedResult<JourneyDto>
            {
                Items = journeyDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> UpdateJourneyAsync(string userId, int journeyId, UpdateJourneyDto dto)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => 
                w.Id == journeyId && w.UserId == userId && !w.IsDeleted);

            if (workflow == null)
                return false;

            workflow.Name = dto.Name;
            workflow.Description = dto.Description;
            workflow.TriggerType = dto.TriggerType;
            workflow.TriggerCriteria = dto.TriggerCriteria;
            workflow.IsActive = dto.IsActive;
            workflow.UpdatedAt = DateTime.UtcNow;

            _workflowRepository.Update(workflow);

            // Delete existing steps
            var existingSteps = await _stepRepository.FindAsync(s => 
                s.WorkflowId == journeyId && !s.IsDeleted);
            
            foreach (var step in existingSteps)
            {
                step.IsDeleted = true;
                step.UpdatedAt = DateTime.UtcNow;
                _stepRepository.Update(step);
            }

            // Create new steps
            foreach (var nodeDto in dto.Nodes)
            {
                var step = new WorkflowStep
                {
                    WorkflowId = workflow.Id,
                    StepOrder = nodeDto.StepOrder,
                    ActionType = nodeDto.ActionType,
                    ActionConfiguration = nodeDto.ActionConfiguration,
                    DelayMinutes = nodeDto.DelayMinutes,
                    PositionX = nodeDto.PositionX,
                    PositionY = nodeDto.PositionY,
                    NodeLabel = nodeDto.NodeLabel,
                    BranchCondition = nodeDto.BranchCondition,
                    NextNodeOnTrue = nodeDto.NextNodeOnTrue,
                    NextNodeOnFalse = nodeDto.NextNodeOnFalse
                };

                await _stepRepository.AddAsync(step);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated journey {JourneyId} for user {UserId}", journeyId, userId);

            return true;
        }

        public async Task<bool> DeleteJourneyAsync(string userId, int journeyId)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => 
                w.Id == journeyId && w.UserId == userId && !w.IsDeleted);

            if (workflow == null)
                return false;

            // Check for active executions
            var activeExecutions = await _executionRepository.FirstOrDefaultAsync(e => 
                e.WorkflowId == journeyId && 
                e.Status == WorkflowExecutionStatus.Running && 
                !e.IsDeleted);

            if (activeExecutions != null)
            {
                _logger.LogWarning("Cannot delete journey {JourneyId} with active executions", journeyId);
                throw new InvalidOperationException("Cannot delete journey with active executions. Please stop all running executions first.");
            }

            workflow.IsDeleted = true;
            workflow.UpdatedAt = DateTime.UtcNow;
            _workflowRepository.Update(workflow);

            // Soft delete steps
            var steps = await _stepRepository.FindAsync(s => 
                s.WorkflowId == journeyId && !s.IsDeleted);
            
            foreach (var step in steps)
            {
                step.IsDeleted = true;
                step.UpdatedAt = DateTime.UtcNow;
                _stepRepository.Update(step);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted journey {JourneyId} for user {UserId}", journeyId, userId);

            return true;
        }

        public async Task<JourneyDto> DuplicateJourneyAsync(string userId, int journeyId)
        {
            var originalJourney = await GetJourneyByIdAsync(userId, journeyId);
            if (originalJourney == null)
                throw new InvalidOperationException($"Journey {journeyId} not found");

            var createDto = new CreateJourneyDto
            {
                Name = $"{originalJourney.Name} (Copy)",
                Description = originalJourney.Description,
                TriggerType = originalJourney.TriggerType,
                TriggerCriteria = originalJourney.TriggerCriteria,
                IsActive = false, // New copy starts as inactive
                Nodes = originalJourney.Nodes.Select(n => new CreateJourneyNodeDto
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

            return await CreateJourneyAsync(userId, createDto);
        }

        public async Task<JourneyStatsDto> GetJourneyStatsAsync(string userId, int journeyId)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => 
                w.Id == journeyId && w.UserId == userId && !w.IsDeleted);

            if (workflow == null)
                throw new InvalidOperationException($"Journey {journeyId} not found");

            var executions = await _executionRepository.FindAsync(e => 
                e.WorkflowId == journeyId && !e.IsDeleted);

            var executionsList = executions.ToList();
            var totalExecutions = executionsList.Count;
            var completedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Completed);
            var failedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Failed);

            var completionRate = totalExecutions > 0 
                ? (double)completedExecutions / totalExecutions * 100 
                : 0;
            
            var failureRate = totalExecutions > 0 
                ? (double)failedExecutions / totalExecutions * 100 
                : 0;

            var completedWithTimes = executionsList
                .Where(e => e.Status == WorkflowExecutionStatus.Completed && 
                           e.StartedAt.HasValue && 
                           e.CompletedAt.HasValue)
                .ToList();

            var avgExecutionTime = completedWithTimes.Any()
                ? completedWithTimes.Average(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalMinutes)
                : 0;

            return new JourneyStatsDto
            {
                JourneyId = journeyId,
                JourneyName = workflow.Name,
                TotalExecutions = totalExecutions,
                ActiveExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Running),
                CompletedExecutions = completedExecutions,
                FailedExecutions = failedExecutions,
                PausedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Paused),
                CompletionRate = completionRate,
                FailureRate = failureRate,
                AverageExecutionTimeMinutes = avgExecutionTime
            };
        }

        public async Task<PaginatedResult<JourneyExecutionDto>> GetJourneyExecutionsAsync(
            string userId, int journeyId, PagedRequest request)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => 
                w.Id == journeyId && w.UserId == userId && !w.IsDeleted);

            if (workflow == null)
                throw new InvalidOperationException($"Journey {journeyId} not found");

            var query = await _executionRepository.FindAsync(e => 
                e.WorkflowId == journeyId && !e.IsDeleted);

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(e => e.StartedAt ?? e.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var executionDtos = new List<JourneyExecutionDto>();
            foreach (var execution in items)
            {
                var contact = await _contactRepository.FirstOrDefaultAsync(c => 
                    c.Id == execution.ContactId && !c.IsDeleted);

                var currentStep = await _stepRepository.FirstOrDefaultAsync(s => 
                    s.WorkflowId == journeyId && 
                    s.StepOrder == execution.CurrentStepOrder && 
                    !s.IsDeleted);

                executionDtos.Add(new JourneyExecutionDto
                {
                    Id = execution.Id,
                    JourneyId = journeyId,
                    JourneyName = workflow.Name,
                    ContactId = execution.ContactId,
                    ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}".Trim() : null,
                    ContactEmail = contact?.Email,
                    Status = execution.Status,
                    CurrentStepOrder = execution.CurrentStepOrder,
                    CurrentStepName = currentStep?.NodeLabel ?? $"Step {execution.CurrentStepOrder}",
                    StartedAt = execution.StartedAt,
                    CompletedAt = execution.CompletedAt,
                    ErrorMessage = execution.ErrorMessage
                });
            }

            return new PaginatedResult<JourneyExecutionDto>
            {
                Items = executionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        // Journey Execution (existing methods)

        public async Task ExecuteWorkflowAsync(int workflowId, int contactId)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => w.Id == workflowId && !w.IsDeleted);
            if (workflow == null || !workflow.IsActive)
            {
                _logger.LogWarning("Workflow {WorkflowId} not found or not active", workflowId);
                return;
            }

            var contact = await _contactRepository.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null)
            {
                _logger.LogWarning("Contact {ContactId} not found", contactId);
                return;
            }

            // Check if there's already an active execution for this workflow and contact
            var existingExecution = await _executionRepository.FirstOrDefaultAsync(e =>
                e.WorkflowId == workflowId &&
                e.ContactId == contactId &&
                e.Status == WorkflowExecutionStatus.Running &&
                !e.IsDeleted);

            if (existingExecution != null)
            {
                _logger.LogInformation("Workflow {WorkflowId} already running for contact {ContactId}", workflowId, contactId);
                return;
            }

            // Create new execution
            var execution = new WorkflowExecution
            {
                WorkflowId = workflowId,
                ContactId = contactId,
                Status = WorkflowExecutionStatus.Running,
                CurrentStepOrder = 0,
                StartedAt = DateTime.UtcNow
            };

            await _executionRepository.AddAsync(execution);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Started workflow {WorkflowId} execution {ExecutionId} for contact {ContactId}",
                workflowId, execution.Id, contactId);

            // Start processing the first step
            BackgroundJob.Enqueue(() => ProcessNextStepAsync(execution.Id));
        }

        public async Task ProcessNextStepAsync(int executionId)
        {
            var execution = await _executionRepository.FirstOrDefaultAsync(e => e.Id == executionId && !e.IsDeleted);
            if (execution == null)
            {
                _logger.LogWarning("Workflow execution {ExecutionId} not found", executionId);
                return;
            }

            if (execution.Status != WorkflowExecutionStatus.Running)
            {
                _logger.LogInformation("Workflow execution {ExecutionId} is not running (status: {Status})", executionId, execution.Status);
                return;
            }

            // Get the next step
            var nextStepOrder = execution.CurrentStepOrder + 1;
            var steps = await _stepRepository.FindAsync(s =>
                s.WorkflowId == execution.WorkflowId &&
                s.StepOrder == nextStepOrder &&
                !s.IsDeleted);

            var step = steps.OrderBy(s => s.StepOrder).FirstOrDefault();

            if (step == null)
            {
                // No more steps, complete the workflow
                execution.Status = WorkflowExecutionStatus.Completed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.UpdatedAt = DateTime.UtcNow;

                _executionRepository.Update(execution);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Workflow execution {ExecutionId} completed", executionId);
                return;
            }

            try
            {
                // Execute the step
                await ExecuteStepAsync(execution, step);

                // Update current step
                execution.CurrentStepOrder = step.StepOrder;
                execution.UpdatedAt = DateTime.UtcNow;

                _executionRepository.Update(execution);
                await _unitOfWork.SaveChangesAsync();

                // Schedule next step if there's a delay
                if (step.DelayMinutes.HasValue && step.DelayMinutes.Value > 0)
                {
                    BackgroundJob.Schedule(() => ProcessNextStepAsync(executionId), TimeSpan.FromMinutes(step.DelayMinutes.Value));
                    _logger.LogInformation("Scheduled next step for execution {ExecutionId} in {Minutes} minutes", executionId, step.DelayMinutes.Value);
                }
                else
                {
                    // Process next step immediately
                    BackgroundJob.Enqueue(() => ProcessNextStepAsync(executionId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing step {StepId} for execution {ExecutionId}", step.Id, executionId);

                execution.Status = WorkflowExecutionStatus.Failed;
                execution.ErrorMessage = ex.Message;
                execution.CompletedAt = DateTime.UtcNow;
                execution.UpdatedAt = DateTime.UtcNow;

                _executionRepository.Update(execution);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task PauseWorkflowExecutionAsync(int executionId)
        {
            var execution = await _executionRepository.FirstOrDefaultAsync(e => e.Id == executionId && !e.IsDeleted);
            if (execution == null)
            {
                _logger.LogWarning("Workflow execution {ExecutionId} not found", executionId);
                return;
            }

            if (execution.Status != WorkflowExecutionStatus.Running)
            {
                _logger.LogWarning("Cannot pause workflow execution {ExecutionId} with status {Status}", executionId, execution.Status);
                return;
            }

            execution.Status = WorkflowExecutionStatus.Paused;
            execution.UpdatedAt = DateTime.UtcNow;

            _executionRepository.Update(execution);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Paused workflow execution {ExecutionId}", executionId);
        }

        public async Task ResumeWorkflowExecutionAsync(int executionId)
        {
            var execution = await _executionRepository.FirstOrDefaultAsync(e => e.Id == executionId && !e.IsDeleted);
            if (execution == null)
            {
                _logger.LogWarning("Workflow execution {ExecutionId} not found", executionId);
                return;
            }

            if (execution.Status != WorkflowExecutionStatus.Paused)
            {
                _logger.LogWarning("Cannot resume workflow execution {ExecutionId} with status {Status}", executionId, execution.Status);
                return;
            }

            execution.Status = WorkflowExecutionStatus.Running;
            execution.UpdatedAt = DateTime.UtcNow;

            _executionRepository.Update(execution);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Resumed workflow execution {ExecutionId}", executionId);

            // Continue processing
            BackgroundJob.Enqueue(() => ProcessNextStepAsync(executionId));
        }

        public async Task CancelWorkflowExecutionAsync(int executionId)
        {
            var execution = await _executionRepository.FirstOrDefaultAsync(e => e.Id == executionId && !e.IsDeleted);
            if (execution == null)
            {
                _logger.LogWarning("Workflow execution {ExecutionId} not found", executionId);
                return;
            }

            execution.Status = WorkflowExecutionStatus.Failed;
            execution.ErrorMessage = "Cancelled by user";
            execution.CompletedAt = DateTime.UtcNow;
            execution.UpdatedAt = DateTime.UtcNow;

            _executionRepository.Update(execution);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cancelled workflow execution {ExecutionId}", executionId);
        }

        public async Task<List<WorkflowExecution>> GetActiveExecutionsForContactAsync(int contactId)
        {
            var executions = await _executionRepository.FindAsync(e =>
                e.ContactId == contactId &&
                e.Status == WorkflowExecutionStatus.Running &&
                !e.IsDeleted);

            return executions.OrderByDescending(e => e.StartedAt).ToList();
        }

        private async Task ExecuteStepAsync(WorkflowExecution execution, WorkflowStep step)
        {
            _logger.LogInformation("Executing step {StepId} (order {Order}, type {Type}) for execution {ExecutionId}",
                step.Id, step.StepOrder, step.ActionType, execution.Id);

            var contact = await _contactRepository.FirstOrDefaultAsync(c => c.Id == execution.ContactId && !c.IsDeleted);
            if (contact == null)
            {
                throw new InvalidOperationException($"Contact {execution.ContactId} not found");
            }

            var workflow = await _workflowRepository.FirstOrDefaultAsync(w => w.Id == execution.WorkflowId && !w.IsDeleted);
            if (workflow == null)
            {
                throw new InvalidOperationException($"Workflow {execution.WorkflowId} not found");
            }

            var config = DeserializeActionConfiguration(step.ActionConfiguration);

            switch (step.ActionType)
            {
                case WorkflowActionType.SendSMS:
                    await SendSMSAsync(contact, workflow.UserId, config);
                    break;

                case WorkflowActionType.SendMMS:
                    await SendMMSAsync(contact, workflow.UserId, config);
                    break;

                case WorkflowActionType.SendEmail:
                    await SendEmailAsync(contact, workflow.UserId, config);
                    break;

                case WorkflowActionType.Wait:
                    // Wait is handled by DelayMinutes
                    break;

                case WorkflowActionType.AddToGroup:
                    await AddToGroupAsync(contact.Id, config);
                    break;

                case WorkflowActionType.RemoveFromGroup:
                    await RemoveFromGroupAsync(contact.Id, config);
                    break;

                case WorkflowActionType.AddTag:
                    await AddTagAsync(contact.Id, config);
                    break;

                default:
                    _logger.LogWarning("Unknown action type {ActionType} for step {StepId}", step.ActionType, step.Id);
                    break;
            }
        }

        private async Task SendSMSAsync(Contact contact, string userId, Dictionary<string, object> config)
        {
            if (string.IsNullOrEmpty(contact.PhoneNumber))
            {
                _logger.LogWarning("Contact {ContactId} has no phone number", contact.Id);
                return;
            }

            var messageBody = config.GetValueOrDefault("messageBody", "")?.ToString() ?? "";
            // TODO: Use MessageService to send SMS
            _logger.LogInformation("Sending SMS to contact {ContactId}: {Message}", contact.Id, messageBody);
        }

        private async Task SendMMSAsync(Contact contact, string userId, Dictionary<string, object> config)
        {
            if (string.IsNullOrEmpty(contact.PhoneNumber))
            {
                _logger.LogWarning("Contact {ContactId} has no phone number", contact.Id);
                return;
            }

            var messageBody = config.GetValueOrDefault("messageBody", "")?.ToString() ?? "";
            // TODO: Use MessageService to send MMS
            _logger.LogInformation("Sending MMS to contact {ContactId}: {Message}", contact.Id, messageBody);
        }

        private async Task SendEmailAsync(Contact contact, string userId, Dictionary<string, object> config)
        {
            if (string.IsNullOrEmpty(contact.Email))
            {
                _logger.LogWarning("Contact {ContactId} has no email", contact.Id);
                return;
            }

            var subject = config.GetValueOrDefault("subject", "")?.ToString() ?? "";
            var messageBody = config.GetValueOrDefault("messageBody", "")?.ToString() ?? "";
            // TODO: Use MessageService to send Email
            _logger.LogInformation("Sending Email to contact {ContactId}: {Subject}", contact.Id, subject);
        }

        private async Task AddToGroupAsync(int contactId, Dictionary<string, object> config)
        {
            if (!config.TryGetValue("groupId", out var groupIdObj) || groupIdObj == null)
            {
                _logger.LogWarning("No groupId in config for AddToGroup action");
                return;
            }

            var groupId = Convert.ToInt32(groupIdObj);

            var existing = await _groupMemberRepository.FirstOrDefaultAsync(gm =>
                gm.ContactId == contactId && gm.ContactGroupId == groupId && !gm.IsDeleted);

            if (existing == null)
            {
                var member = new ContactGroupMember
                {
                    ContactId = contactId,
                    ContactGroupId = groupId
                };

                await _groupMemberRepository.AddAsync(member);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added contact {ContactId} to group {GroupId}", contactId, groupId);
            }
        }

        private async Task RemoveFromGroupAsync(int contactId, Dictionary<string, object> config)
        {
            if (!config.TryGetValue("groupId", out var groupIdObj) || groupIdObj == null)
            {
                _logger.LogWarning("No groupId in config for RemoveFromGroup action");
                return;
            }

            var groupId = Convert.ToInt32(groupIdObj);

            var member = await _groupMemberRepository.FirstOrDefaultAsync(gm =>
                gm.ContactId == contactId && gm.ContactGroupId == groupId && !gm.IsDeleted);

            if (member != null)
            {
                member.IsDeleted = true;
                member.UpdatedAt = DateTime.UtcNow;

                _groupMemberRepository.Update(member);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Removed contact {ContactId} from group {GroupId}", contactId, groupId);
            }
        }

        private async Task AddTagAsync(int contactId, Dictionary<string, object> config)
        {
            if (!config.TryGetValue("tagId", out var tagIdObj) || tagIdObj == null)
            {
                _logger.LogWarning("No tagId in config for AddTag action");
                return;
            }

            var tagId = Convert.ToInt32(tagIdObj);

            var existing = await _tagAssignmentRepository.FirstOrDefaultAsync(ta =>
                ta.ContactId == contactId && ta.ContactTagId == tagId && !ta.IsDeleted);

            if (existing == null)
            {
                var assignment = new ContactTagAssignment
                {
                    ContactId = contactId,
                    ContactTagId = tagId
                };

                await _tagAssignmentRepository.AddAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added tag {TagId} to contact {ContactId}", tagId, contactId);
            }
        }

        private Dictionary<string, object> DeserializeActionConfiguration(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, object>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize action configuration: {Json}", json);
                return new Dictionary<string, object>();
            }
        }
    }
}
