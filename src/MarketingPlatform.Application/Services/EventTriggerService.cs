using Hangfire;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    /// <summary>
    /// Event trigger service — ALL workflow matching is scoped to the contact's UserId.
    /// User A's workflows will NEVER fire for User B's contacts.
    /// </summary>
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

        /// <summary>
        /// Trigger workflows based on an event.
        /// CRITICAL: Only triggers workflows belonging to the same user who owns the contact.
        /// </summary>
        public async Task TriggerEventAsync(EventType eventType, int contactId, Dictionary<string, object>? eventData = null)
        {
            _logger.LogInformation("Triggering event {EventType} for contact {ContactId}", eventType, contactId);

            // ── Resolve the contact's owner (UserId) — this is the isolation boundary ──
            var contact = await _contactRepository.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null)
            {
                _logger.LogWarning("Contact {ContactId} not found — cannot trigger workflows", contactId);
                return;
            }

            var userId = contact.UserId;
            _logger.LogInformation("Contact {ContactId} belongs to user {UserId} — searching only their workflows", contactId, userId);

            // ── 1. Match workflows with TriggerType = Event for THIS user only ──
            var eventWorkflows = await _workflowRepository.FindAsync(w =>
                w.UserId == userId &&
                w.TriggerType == TriggerType.Event &&
                w.IsActive &&
                !w.IsDeleted);

            foreach (var workflow in eventWorkflows)
            {
                if (DoesEventMatchCriteria(workflow.TriggerCriteria, eventType, eventData))
                {
                    BackgroundJob.Enqueue(() => _workflowService.ExecuteWorkflowAsync(workflow.Id, contactId));
                    _logger.LogInformation("Queued Event-workflow {WorkflowId} (user {UserId}) for contact {ContactId} due to {EventType}",
                        workflow.Id, userId, contactId, eventType);
                }
            }

            // ── 2. Match workflows with TriggerType = Keyword for THIS user only ──
            if (eventType == EventType.KeywordReceived && eventData != null)
            {
                var incomingKeyword = eventData.TryGetValue("keyword", out var kwObj) ? kwObj?.ToString() : null;

                if (!string.IsNullOrEmpty(incomingKeyword))
                {
                    var keywordWorkflows = await _workflowRepository.FindAsync(w =>
                        w.UserId == userId &&
                        w.TriggerType == TriggerType.Keyword &&
                        w.IsActive &&
                        !w.IsDeleted);

                    foreach (var workflow in keywordWorkflows)
                    {
                        if (DoesKeywordMatch(workflow.TriggerCriteria, incomingKeyword))
                        {
                            BackgroundJob.Enqueue(() => _workflowService.ExecuteWorkflowAsync(workflow.Id, contactId));
                            _logger.LogInformation(
                                "Queued Keyword-workflow {WorkflowId} (user {UserId}) for contact {ContactId} — keyword '{Keyword}' matched",
                                workflow.Id, userId, contactId, incomingKeyword);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check inactivity triggers — already scoped: each workflow only queries its own user's contacts.
        /// </summary>
        public async Task CheckInactivityTriggersAsync()
        {
            _logger.LogInformation("Checking inactivity triggers");

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

                if (!criteria.TryGetValue("inactiveDays", out var inactiveDaysObj) || inactiveDaysObj == null)
                    continue;

                var inactiveDays = Convert.ToInt32(inactiveDaysObj);
                var thresholdDate = DateTime.UtcNow.AddDays(-inactiveDays);

                // Only query contacts belonging to this workflow's owner
                var contacts = await _contactRepository.FindAsync(c =>
                    c.UserId == workflow.UserId &&
                    c.IsActive &&
                    !c.IsDeleted);

                var batchSize = 100;
                var contactList = contacts.ToList();

                for (int i = 0; i < contactList.Count; i += batchSize)
                {
                    var batch = contactList.Skip(i).Take(batchSize).ToList();

                    foreach (var contact in batch)
                    {
                        if (contact.UpdatedAt < thresholdDate)
                        {
                            BackgroundJob.Enqueue(() => _workflowService.ExecuteWorkflowAsync(workflow.Id, contact.Id));
                            _logger.LogInformation("Queued workflow {WorkflowId} for inactive contact {ContactId} (user {UserId})",
                                workflow.Id, contact.Id, workflow.UserId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process keyword trigger — scoped: finds keyword for the contact's user, then triggers workflows.
        /// </summary>
        public async Task ProcessKeywordTriggerAsync(string keyword, int contactId)
        {
            _logger.LogInformation("Processing keyword trigger '{Keyword}' for contact {ContactId}", keyword, contactId);

            // Resolve the contact's owner
            var contact = await _contactRepository.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null)
            {
                _logger.LogWarning("Contact {ContactId} not found — cannot process keyword trigger", contactId);
                return;
            }

            var userId = contact.UserId;

            // Find keyword belonging to the same user (case-insensitive)
            var keywords = await _keywordRepository.FindAsync(k =>
                k.UserId == userId &&
                k.KeywordText.ToLower() == keyword.ToLower() &&
                k.Status == KeywordStatus.Active &&
                !k.IsDeleted);

            var keywordEntity = keywords.FirstOrDefault();
            if (keywordEntity == null)
            {
                _logger.LogInformation("Keyword '{Keyword}' not found for user {UserId}", keyword, userId);
                return;
            }

            // Log keyword activity
            var activity = new KeywordActivity
            {
                KeywordId = keywordEntity.Id,
                PhoneNumber = contact.PhoneNumber ?? "",
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

                    _logger.LogInformation("Added contact {ContactId} to opt-in group {GroupId} (user {UserId})",
                        contactId, keywordEntity.OptInGroupId.Value, userId);
                }
            }

            // Trigger workflows — TriggerEventAsync already scopes to contact's UserId
            var eventData = new Dictionary<string, object>
            {
                { "keyword", keyword },
                { "keywordId", keywordEntity.Id }
            };

            BackgroundJob.Enqueue(() => TriggerEventAsync(EventType.KeywordReceived, contactId, eventData));

            if (keywordEntity.LinkedCampaignId.HasValue)
            {
                _logger.LogInformation("Keyword '{Keyword}' linked to campaign {CampaignId} (user {UserId})",
                    keyword, keywordEntity.LinkedCampaignId.Value, userId);
            }
        }

        public async Task RegisterCustomEventAsync(string eventName, int contactId, Dictionary<string, object>? eventData = null)
        {
            _logger.LogInformation("Registering custom event '{EventName}' for contact {ContactId}", eventName, contactId);

            var data = eventData ?? new Dictionary<string, object>();
            data["customEventName"] = eventName;

            BackgroundJob.Enqueue(() => TriggerEventAsync(EventType.Custom, contactId, data));
        }

        // ───────────────────────────────────────────────────────────
        //  Private matching logic
        // ───────────────────────────────────────────────────────────

        private bool DoesEventMatchCriteria(string? triggerCriteria, EventType eventType, Dictionary<string, object>? eventData)
        {
            if (string.IsNullOrEmpty(triggerCriteria))
                return false;

            var criteria = DeserializeTriggerCriteria(triggerCriteria);

            if (criteria.TryGetValue("eventType", out var expectedEventTypeObj) && expectedEventTypeObj != null)
            {
                var expectedEventType = expectedEventTypeObj.ToString();
                if (!string.Equals(expectedEventType, eventType.ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            else
            {
                return false;
            }

            // Optional: keyword filter within Event workflow
            if (eventType == EventType.KeywordReceived &&
                criteria.TryGetValue("keyword", out var criteriaKeyword) && criteriaKeyword != null &&
                eventData != null &&
                eventData.TryGetValue("keyword", out var incomingKeyword) && incomingKeyword != null)
            {
                if (!string.Equals(criteriaKeyword.ToString(), incomingKeyword.ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Optional: groupId filter
            if (criteria.TryGetValue("groupId", out var criteriaGroupId) && criteriaGroupId != null &&
                eventData != null &&
                eventData.TryGetValue("groupId", out var incomingGroupId) && incomingGroupId != null)
            {
                if (criteriaGroupId.ToString() != incomingGroupId.ToString())
                    return false;
            }

            // Optional: tagId filter
            if (criteria.TryGetValue("tagId", out var criteriaTagId) && criteriaTagId != null &&
                eventData != null &&
                eventData.TryGetValue("tagId", out var incomingTagId) && incomingTagId != null)
            {
                if (criteriaTagId.ToString() != incomingTagId.ToString())
                    return false;
            }

            return true;
        }

        private bool DoesKeywordMatch(string? triggerCriteria, string incomingKeyword)
        {
            if (string.IsNullOrEmpty(triggerCriteria) || string.IsNullOrEmpty(incomingKeyword))
                return false;

            var trimmedCriteria = triggerCriteria.Trim();

            if (trimmedCriteria.StartsWith("{"))
            {
                try
                {
                    var criteria = JsonConvert.DeserializeObject<Dictionary<string, object>>(trimmedCriteria);
                    if (criteria == null)
                        return false;

                    if (criteria.TryGetValue("keyword", out var singleKw) && singleKw != null)
                    {
                        if (string.Equals(singleKw.ToString(), incomingKeyword, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    if (criteria.TryGetValue("keywords", out var multiKw) && multiKw != null)
                    {
                        var keywordList = JsonConvert.DeserializeObject<List<string>>(multiKw.ToString()!);
                        if (keywordList != null && keywordList.Any(k =>
                            string.Equals(k, incomingKeyword, StringComparison.OrdinalIgnoreCase)))
                            return true;
                    }

                    return false;
                }
                catch
                {
                    // Not valid JSON, fall through
                }
            }

            var keywordTexts = trimmedCriteria
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return keywordTexts.Any(k => string.Equals(k, incomingKeyword, StringComparison.OrdinalIgnoreCase));
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
