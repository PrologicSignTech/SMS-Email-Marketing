using Hangfire;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class EventTriggerService : IEventTriggerService
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<Keyword> _keywordRepository;
        private readonly IRepository<KeywordActivity> _keywordActivityRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IWorkflowService _workflowService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EventTriggerService> _logger;

        public EventTriggerService(
            IRepository<Workflow> workflowRepository,
            IRepository<Contact> contactRepository,
            IRepository<Keyword> keywordRepository,
            IRepository<KeywordActivity> keywordActivityRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IWorkflowService workflowService,
            IUnitOfWork unitOfWork,
            ILogger<EventTriggerService> logger)
        {
            _workflowRepository = workflowRepository;
            _contactRepository = contactRepository;
            _keywordRepository = keywordRepository;
            _keywordActivityRepository = keywordActivityRepository;
            _groupMemberRepository = groupMemberRepository;
            _workflowService = workflowService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task TriggerEventAsync(EventType eventType, int contactId, Dictionary<string, object>? eventData = null)
        {
            _logger.LogInformation("Triggering event {EventType} for contact {ContactId}", eventType, contactId);

            // Find all active workflows that match this trigger type
            var workflows = await _workflowRepository.FindAsync(w =>
                w.TriggerType == TriggerType.Event &&
                w.IsActive &&
                !w.IsDeleted);

            foreach (var workflow in workflows)
            {
                // Parse trigger criteria to check if this event matches
                if (DoesEventMatchCriteria(workflow.TriggerCriteria, eventType, eventData))
                {
                    // Execute workflow asynchronously using Hangfire to avoid blocking
                    BackgroundJob.Enqueue(() => _workflowService.ExecuteWorkflowAsync(workflow.Id, contactId));
                    _logger.LogInformation("Queued workflow {WorkflowId} for contact {ContactId} due to event {EventType}",
                        workflow.Id, contactId, eventType);
                }
            }
        }

        public async Task CheckInactivityTriggersAsync()
        {
            _logger.LogInformation("Checking inactivity triggers");

            // Find all active inactivity workflows
            var workflows = await _workflowRepository.FindAsync(w =>
                w.TriggerType == TriggerType.Event &&
                w.IsActive &&
                !w.IsDeleted);

            foreach (var workflow in workflows)
            {
                var criteria = DeserializeTriggerCriteria(workflow.TriggerCriteria);
                
                if (!criteria.TryGetValue("eventType", out var eventTypeObj) || eventTypeObj == null)
                    continue;

                var eventType = eventTypeObj.ToString();
                if (eventType != EventType.Inactivity.ToString())
                    continue;

                // Get inactivity threshold in days
                if (!criteria.TryGetValue("inactiveDays", out var inactiveDaysObj) || inactiveDaysObj == null)
                    continue;

                var inactiveDays = Convert.ToInt32(inactiveDaysObj);
                var thresholdDate = DateTime.UtcNow.AddDays(-inactiveDays);

                // Find contacts who haven't been messaged since threshold
                // Use AsNoTracking and process in batches to avoid DB locks
                var contacts = await _contactRepository.FindAsync(c =>
                    c.UserId == workflow.UserId &&
                    c.IsActive &&
                    !c.IsDeleted);

                // Process in batches to avoid DB load
                var batchSize = 100;
                var contactList = contacts.ToList();
                
                for (int i = 0; i < contactList.Count; i += batchSize)
                {
                    var batch = contactList.Skip(i).Take(batchSize).ToList();
                    
                    foreach (var contact in batch)
                    {
                        // Check if contact has been inactive
                        if (contact.UpdatedAt < thresholdDate)
                        {
                            // Queue workflow execution without blocking
                            BackgroundJob.Enqueue(() => _workflowService.ExecuteWorkflowAsync(workflow.Id, contact.Id));
                            _logger.LogInformation("Queued workflow {WorkflowId} for inactive contact {ContactId}", workflow.Id, contact.Id);
                        }
                    }
                }
            }
        }

        public async Task ProcessKeywordTriggerAsync(string keyword, int contactId)
        {
            _logger.LogInformation("Processing keyword trigger '{Keyword}' for contact {ContactId}", keyword, contactId);

            // Find the keyword (case-insensitive)
            var keywords = await _keywordRepository.FindAsync(k =>
                k.KeywordText.ToLower() == keyword.ToLower() &&
                k.Status == KeywordStatus.Active &&
                !k.IsDeleted);

            var keywordEntity = keywords.FirstOrDefault();
            if (keywordEntity == null)
            {
                _logger.LogWarning("Keyword '{Keyword}' not found or not active", keyword);
                return;
            }

            // Log keyword activity (non-blocking)
            var activity = new KeywordActivity
            {
                KeywordId = keywordEntity.Id,
                PhoneNumber = "", // Will be filled by caller
                IncomingMessage = keyword,
                ResponseSent = keywordEntity.ResponseMessage,
                ReceivedAt = DateTime.UtcNow
            };

            await _keywordActivityRepository.AddAsync(activity);
            await _unitOfWork.SaveChangesAsync();

            // Add contact to opt-in group if specified
            if (keywordEntity.OptInGroupId.HasValue)
            {
                var existing = await _groupMemberRepository.FirstOrDefaultAsync(gm =>
                    gm.ContactId == contactId &&
                    gm.ContactGroupId == keywordEntity.OptInGroupId.Value &&
                    !gm.IsDeleted);

                if (existing == null)
                {
                    var member = new ContactGroupMember
                    {
                        ContactId = contactId,
                        ContactGroupId = keywordEntity.OptInGroupId.Value
                    };

                    await _groupMemberRepository.AddAsync(member);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Added contact {ContactId} to opt-in group {GroupId}",
                        contactId, keywordEntity.OptInGroupId.Value);
                }
            }

            // Trigger workflows that listen for keyword events
            var eventData = new Dictionary<string, object>
            {
                { "keyword", keyword },
                { "keywordId", keywordEntity.Id }
            };

            // Use background job to avoid blocking
            BackgroundJob.Enqueue(() => TriggerEventAsync(EventType.KeywordReceived, contactId, eventData));

            // Start linked campaign if specified
            if (keywordEntity.LinkedCampaignId.HasValue)
            {
                _logger.LogInformation("Keyword '{Keyword}' linked to campaign {CampaignId}",
                    keyword, keywordEntity.LinkedCampaignId.Value);
                // Campaign will be started by the workflow or separately
            }
        }

        public async Task RegisterCustomEventAsync(string eventName, int contactId, Dictionary<string, object>? eventData = null)
        {
            _logger.LogInformation("Registering custom event '{EventName}' for contact {ContactId}", eventName, contactId);

            var data = eventData ?? new Dictionary<string, object>();
            data["customEventName"] = eventName;

            // Use background job to avoid blocking
            BackgroundJob.Enqueue(() => TriggerEventAsync(EventType.Custom, contactId, data));
        }

        private bool DoesEventMatchCriteria(string? triggerCriteria, EventType eventType, Dictionary<string, object>? eventData)
        {
            if (string.IsNullOrEmpty(triggerCriteria))
                return false;

            var criteria = DeserializeTriggerCriteria(triggerCriteria);

            // Check if event type matches
            if (criteria.TryGetValue("eventType", out var expectedEventTypeObj) && expectedEventTypeObj != null)
            {
                var expectedEventType = expectedEventTypeObj.ToString();
                if (expectedEventType != eventType.ToString())
                    return false;
            }

            // Check custom criteria if present
            if (eventData != null && criteria.TryGetValue("customCriteria", out var customCriteriaObj) && customCriteriaObj != null)
            {
                // Custom criteria matching logic can be extended here
                // For now, we just check if the event type matches
            }

            return true;
        }

        private Dictionary<string, object> DeserializeTriggerCriteria(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, object>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize trigger criteria: {Json}", json);
                return new Dictionary<string, object>();
            }
        }
    }
}
