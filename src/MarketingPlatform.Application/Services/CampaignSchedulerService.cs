using Hangfire;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace MarketingPlatform.Application.Services
{
    public class CampaignSchedulerService : ICampaignSchedulerService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignSchedule> _scheduleRepository;
        private readonly IRepository<CampaignAudience> _audienceRepository;
        private readonly IRepository<CampaignContent> _contentRepository;
        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IRateLimitService _rateLimitService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CampaignSchedulerService> _logger;

        public CampaignSchedulerService(
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignSchedule> scheduleRepository,
            IRepository<CampaignAudience> audienceRepository,
            IRepository<CampaignContent> contentRepository,
            IRepository<CampaignMessage> messageRepository,
            IRepository<Contact> contactRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IRateLimitService rateLimitService,
            IUnitOfWork unitOfWork,
            ILogger<CampaignSchedulerService> logger)
        {
            _campaignRepository = campaignRepository;
            _scheduleRepository = scheduleRepository;
            _audienceRepository = audienceRepository;
            _contentRepository = contentRepository;
            _messageRepository = messageRepository;
            _contactRepository = contactRepository;
            _groupMemberRepository = groupMemberRepository;
            _rateLimitService = rateLimitService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ScheduleCampaignAsync(int campaignId, DateTime scheduledDateTime, string? timeZone = null)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            // Update campaign status
            campaign.Status = CampaignStatus.Scheduled;
            campaign.ScheduledAt = scheduledDateTime;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);

            // Update or create schedule
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaignId && !s.IsDeleted);
            if (schedule == null)
            {
                schedule = new CampaignSchedule
                {
                    CampaignId = campaignId,
                    ScheduleType = ScheduleType.OneTime,
                    ScheduledDateTime = scheduledDateTime,
                    TimeZoneAware = !string.IsNullOrEmpty(timeZone),
                    TimeZone = timeZone
                };
                await _scheduleRepository.AddAsync(schedule);
            }
            else
            {
                schedule.ScheduleType = ScheduleType.OneTime;
                schedule.ScheduledDateTime = scheduledDateTime;
                schedule.TimeZoneAware = !string.IsNullOrEmpty(timeZone);
                schedule.TimeZone = timeZone;
                schedule.UpdatedAt = DateTime.UtcNow;
                _scheduleRepository.Update(schedule);
            }

            await _unitOfWork.SaveChangesAsync();

            // Calculate execution time with timezone conversion
            var executionTime = scheduledDateTime;
            if (!string.IsNullOrEmpty(timeZone))
            {
                try
                {
                    var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    // Treat scheduledDateTime as local time in target timezone and convert to UTC
                    executionTime = TimeZoneInfo.ConvertTimeToUtc(scheduledDateTime, targetTimeZone);
                }
                catch (TimeZoneNotFoundException ex)
                {
                    _logger.LogError(ex, "Invalid timezone {TimeZone}, using UTC", timeZone);
                }
            }

            // Schedule Hangfire job - this is non-blocking and doesn't lock DB
            var jobId = BackgroundJob.Schedule(
                () => ProcessScheduledCampaignAsync(campaignId),
                executionTime);

            _logger.LogInformation("Scheduled campaign {CampaignId} for execution at {ExecutionTime} UTC (job {JobId})",
                campaignId, executionTime, jobId);
        }

        public async Task ScheduleRecurringCampaignAsync(int campaignId, string cronExpression, string? timeZone = null)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            // Update campaign status
            campaign.Status = CampaignStatus.Scheduled;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);

            // Update or create schedule
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaignId && !s.IsDeleted);
            if (schedule == null)
            {
                schedule = new CampaignSchedule
                {
                    CampaignId = campaignId,
                    ScheduleType = ScheduleType.Recurring,
                    RecurrencePattern = cronExpression,
                    TimeZoneAware = !string.IsNullOrEmpty(timeZone),
                    TimeZone = timeZone
                };
                await _scheduleRepository.AddAsync(schedule);
            }
            else
            {
                schedule.ScheduleType = ScheduleType.Recurring;
                schedule.RecurrencePattern = cronExpression;
                schedule.TimeZoneAware = !string.IsNullOrEmpty(timeZone);
                schedule.TimeZone = timeZone;
                schedule.UpdatedAt = DateTime.UtcNow;
                _scheduleRepository.Update(schedule);
            }

            await _unitOfWork.SaveChangesAsync();

            // Create recurring job with unique ID
            var jobId = $"recurring-campaign-{campaignId}";
            
            if (!string.IsNullOrEmpty(timeZone))
            {
                RecurringJob.AddOrUpdate(
                    jobId,
                    () => ProcessRecurringCampaignAsync(campaignId),
                    cronExpression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone)
                    });
            }
            else
            {
                RecurringJob.AddOrUpdate(
                    jobId,
                    () => ProcessRecurringCampaignAsync(campaignId),
                    cronExpression);
            }

            _logger.LogInformation("Scheduled recurring campaign {CampaignId} with cron '{Cron}' (job {JobId})",
                campaignId, cronExpression, jobId);
        }

        public async Task ScheduleDripCampaignAsync(int campaignId, int workflowId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            // Update campaign status
            campaign.Status = CampaignStatus.Scheduled;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);

            // Update or create schedule
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaignId && !s.IsDeleted);
            if (schedule == null)
            {
                schedule = new CampaignSchedule
                {
                    CampaignId = campaignId,
                    ScheduleType = ScheduleType.Drip,
                    RecurrencePattern = $"{{\"workflowId\":{workflowId}}}"
                };
                await _scheduleRepository.AddAsync(schedule);
            }
            else
            {
                schedule.ScheduleType = ScheduleType.Drip;
                schedule.RecurrencePattern = $"{{\"workflowId\":{workflowId}}}";
                schedule.UpdatedAt = DateTime.UtcNow;
                _scheduleRepository.Update(schedule);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Scheduled drip campaign {CampaignId} with workflow {WorkflowId}",
                campaignId, workflowId);

            // Drip campaigns are triggered by workflow execution, not by scheduler
        }

        public async Task CancelScheduledCampaignAsync(int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            // Update campaign status
            campaign.Status = CampaignStatus.Failed; // Or Draft
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            // Remove recurring job if it exists
            var recurringJobId = $"recurring-campaign-{campaignId}";
            RecurringJob.RemoveIfExists(recurringJobId);

            _logger.LogInformation("Cancelled scheduled campaign {CampaignId}", campaignId);
        }

        public async Task ProcessScheduledCampaignAsync(int campaignId)
        {
            _logger.LogInformation("Processing scheduled campaign {CampaignId}", campaignId);

            // Use minimal SELECT to avoid locking entire row
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            if (campaign.Status != CampaignStatus.Scheduled)
            {
                _logger.LogWarning("Campaign {CampaignId} is not in Scheduled status (current: {Status})",
                    campaignId, campaign.Status);
                return;
            }

            // Update status atomically to avoid race conditions
            campaign.Status = CampaignStatus.Running;
            campaign.StartedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                // Get audience in batches to avoid DB locks
                var audience = await _audienceRepository.FirstOrDefaultAsync(a => a.CampaignId == campaignId && !a.IsDeleted);
                if (audience == null)
                {
                    _logger.LogWarning("Campaign {CampaignId} has no audience", campaignId);
                    return;
                }

                var contactIds = await GetAudienceContactIdsAsync(campaign.UserId, audience);

                _logger.LogInformation("Campaign {CampaignId} will be sent to {Count} contacts", campaignId, contactIds.Count);

                // Process contacts in batches to avoid DB load and locks
                var batchSize = 100;
                for (int i = 0; i < contactIds.Count; i += batchSize)
                {
                    var batchIds = contactIds.Skip(i).Take(batchSize).ToList();
                    
                    // Queue batch processing as separate background job
                    BackgroundJob.Enqueue(() => ProcessCampaignBatchAsync(campaignId, batchIds));
                    
                    _logger.LogInformation("Queued batch {Batch}/{Total} for campaign {CampaignId}",
                        (i / batchSize) + 1, (contactIds.Count + batchSize - 1) / batchSize, campaignId);
                }

                // Update campaign counts
                campaign.TotalRecipients = contactIds.Count;
                campaign.UpdatedAt = DateTime.UtcNow;

                _campaignRepository.Update(campaign);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing campaign {CampaignId}", campaignId);

                campaign.Status = CampaignStatus.Failed;
                campaign.CompletedAt = DateTime.UtcNow;
                campaign.UpdatedAt = DateTime.UtcNow;

                _campaignRepository.Update(campaign);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ProcessRecurringCampaignAsync(int campaignId)
        {
            _logger.LogInformation("Processing recurring campaign iteration for {CampaignId}", campaignId);

            // For recurring campaigns, we just trigger a new execution
            // This avoids conflicts with the original campaign status
            await ProcessScheduledCampaignAsync(campaignId);

            // Reset status back to Scheduled for next iteration
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign != null && campaign.Status == CampaignStatus.Completed)
            {
                campaign.Status = CampaignStatus.Scheduled;
                campaign.UpdatedAt = DateTime.UtcNow;

                _campaignRepository.Update(campaign);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // Public method for Hangfire to call
        public async Task ProcessCampaignBatchAsync(int campaignId, List<int> contactIds)
        {
            _logger.LogInformation("Processing batch of {Count} contacts for campaign {CampaignId}",
                contactIds.Count, campaignId);

            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return;
            }

            var content = await _contentRepository.FirstOrDefaultAsync(c => c.CampaignId == campaignId && !c.IsDeleted);
            if (content == null)
            {
                _logger.LogWarning("Campaign {CampaignId} has no content", campaignId);
                return;
            }

            // Get contacts in this batch
            var contacts = await _contactRepository.FindAsync(c =>
                contactIds.Contains(c.Id) && !c.IsDeleted);

            foreach (var contact in contacts)
            {
                try
                {
                    // Check rate limits before sending
                    var canSend = await _rateLimitService.CanSendMessageAsync(contact.Id, campaign.UserId);
                    if (!canSend)
                    {
                        _logger.LogWarning("Rate limit exceeded for contact {ContactId}", contact.Id);
                        continue;
                    }

                    // Create campaign message record
                    var message = new CampaignMessage
                    {
                        CampaignId = campaignId,
                        ContactId = contact.Id,
                        Channel = content.Channel,
                        Recipient = content.Channel == ChannelType.Email ? contact.Email : contact.PhoneNumber,
                        Subject = content.Subject,
                        MessageBody = content.MessageBody,
                        Status = MessageStatus.Queued,
                        ScheduledAt = DateTime.UtcNow
                    };

                    await _messageRepository.AddAsync(message);

                    // Record message sent for rate limiting
                    await _rateLimitService.RecordMessageSentAsync(contact.Id, campaign.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating message for contact {ContactId} in campaign {CampaignId}",
                        contact.Id, campaignId);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Completed batch processing for campaign {CampaignId}", campaignId);
        }

        private async Task<List<int>> GetAudienceContactIdsAsync(string userId, CampaignAudience audience)
        {
            var contactIds = new List<int>();

            if (audience.TargetType == TargetType.All)
            {
                // Get all contacts - use Select to minimize data transfer
                var contacts = await _contactRepository.FindAsync(c => c.UserId == userId && !c.IsDeleted);
                contactIds = contacts.Select(c => c.Id).ToList();
            }
            else if (audience.TargetType == TargetType.Groups && !string.IsNullOrEmpty(audience.GroupIds))
            {
                // Parse group IDs
                var groupIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(audience.GroupIds);
                if (groupIds != null && groupIds.Any())
                {
                    // Get contacts from groups - use Select to minimize data transfer
                    var groupMembers = await _groupMemberRepository.FindAsync(gm =>
                        groupIds.Contains(gm.ContactGroupId) && !gm.IsDeleted);

                    contactIds = groupMembers.Select(gm => gm.ContactId).Distinct().ToList();
                }
            }
            else if (audience.TargetType == TargetType.Segments)
            {
                // TODO: Implement segment criteria evaluation
                _logger.LogWarning("Segment targeting not yet implemented");
            }

            return contactIds;
        }
    }
}
