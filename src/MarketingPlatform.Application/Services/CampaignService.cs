using AutoMapper;
using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignContent> _contentRepository;
        private readonly IRepository<CampaignAudience> _audienceRepository;
        private readonly IRepository<CampaignSchedule> _scheduleRepository;
        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CampaignService> _logger;

        public CampaignService(
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignContent> contentRepository,
            IRepository<CampaignAudience> audienceRepository,
            IRepository<CampaignSchedule> scheduleRepository,
            IRepository<CampaignMessage> messageRepository,
            IRepository<Contact> contactRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CampaignService> logger)
        {
            _campaignRepository = campaignRepository;
            _contentRepository = contentRepository;
            _audienceRepository = audienceRepository;
            _scheduleRepository = scheduleRepository;
            _messageRepository = messageRepository;
            _contactRepository = contactRepository;
            _groupMemberRepository = groupMemberRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CampaignDto?> GetCampaignByIdAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return null;

            return await MapToCampaignDtoAsync(campaign);
        }

        public async Task<PaginatedResult<CampaignDto>> GetCampaignsAsync(string userId, PagedRequest request)
        {
            var query = (await _campaignRepository.FindAsync(c =>
                c.UserId == userId && !c.IsDeleted)).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.SearchTerm) ||
                    (c.Description != null && c.Description.Contains(request.SearchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                    "status" => request.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                    "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    _ => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var campaigns = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var campaignDtos = new List<CampaignDto>();
            foreach (var campaign in campaigns)
            {
                campaignDtos.Add(await MapToCampaignDtoAsync(campaign));
            }

            return new PaginatedResult<CampaignDto>
            {
                Items = campaignDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<List<CampaignDto>> GetCampaignsByStatusAsync(string userId, CampaignStatus status)
        {
            var campaigns = await _campaignRepository.FindAsync(c =>
                c.UserId == userId && c.Status == status && !c.IsDeleted);

            var campaignDtos = new List<CampaignDto>();
            foreach (var campaign in campaigns.OrderByDescending(c => c.CreatedAt))
            {
                campaignDtos.Add(await MapToCampaignDtoAsync(campaign));
            }

            return campaignDtos;
        }

        public async Task<CampaignDto> CreateCampaignAsync(string userId, CreateCampaignDto dto)
        {
            var campaign = _mapper.Map<Campaign>(dto);
            campaign.UserId = userId;
            campaign.Status = CampaignStatus.Draft;

            await _campaignRepository.AddAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            // Create campaign content
            var content = new CampaignContent
            {
                CampaignId = campaign.Id,
                Channel = dto.Content.Channel,
                Subject = dto.Content.Subject,
                MessageBody = dto.Content.MessageBody,
                HTMLContent = dto.Content.HTMLContent,
                MediaUrls = SerializeJson(dto.Content.MediaUrls),
                MessageTemplateId = dto.Content.TemplateId,
                PersonalizationTokens = SerializeJson(dto.Content.PersonalizationTokens)
            };
            await _contentRepository.AddAsync(content);

            // Create campaign audience
            var audience = new CampaignAudience
            {
                CampaignId = campaign.Id,
                TargetType = dto.Audience.TargetType,
                GroupIds = SerializeJson(dto.Audience.GroupIds),
                SegmentCriteria = dto.Audience.SegmentCriteria,
                ExclusionListIds = SerializeJson(dto.Audience.ExclusionListIds)
            };
            await _audienceRepository.AddAsync(audience);

            // Create campaign schedule if provided
            if (dto.Schedule != null)
            {
                var schedule = new CampaignSchedule
                {
                    CampaignId = campaign.Id,
                    ScheduleType = dto.Schedule.ScheduleType,
                    ScheduledDateTime = dto.Schedule.ScheduledDate,
                    RecurrencePattern = dto.Schedule.RecurrenceRule,
                    TimeZoneAware = dto.Schedule.TimeZoneAware,
                    TimeZone = dto.Schedule.PreferredTimeZone
                };
                await _scheduleRepository.AddAsync(schedule);
            }

            await _unitOfWork.SaveChangesAsync();

            var createdCampaign = await GetCampaignByIdAsync(userId, campaign.Id);
            if (createdCampaign == null)
            {
                _logger.LogError("Failed to retrieve created campaign {CampaignId}", campaign.Id);
                throw new InvalidOperationException($"Failed to retrieve created campaign with ID {campaign.Id}");
            }

            return createdCampaign;
        }

        public async Task<bool> UpdateCampaignAsync(string userId, int campaignId, UpdateCampaignDto dto)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Can only update Draft campaigns
            if (campaign.Status != CampaignStatus.Draft)
            {
                _logger.LogWarning("Attempted to update non-draft campaign {CampaignId}", campaignId);
                return false;
            }

            // Update campaign basic info
            campaign.Name = dto.Name;
            campaign.Description = dto.Description;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);

            // Update content if provided
            if (dto.Content != null)
            {
                var content = await _contentRepository.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
                if (content != null)
                {
                    content.Channel = dto.Content.Channel;
                    content.Subject = dto.Content.Subject;
                    content.MessageBody = dto.Content.MessageBody;
                    content.HTMLContent = dto.Content.HTMLContent;
                    content.MediaUrls = SerializeJson(dto.Content.MediaUrls);
                    content.MessageTemplateId = dto.Content.TemplateId;
                    content.PersonalizationTokens = SerializeJson(dto.Content.PersonalizationTokens);
                    content.UpdatedAt = DateTime.UtcNow;

                    _contentRepository.Update(content);
                }
            }

            // Update audience if provided
            if (dto.Audience != null)
            {
                var audience = await _audienceRepository.FirstOrDefaultAsync(a => a.CampaignId == campaignId);
                if (audience != null)
                {
                    audience.TargetType = dto.Audience.TargetType;
                    audience.GroupIds = SerializeJson(dto.Audience.GroupIds);
                    audience.SegmentCriteria = dto.Audience.SegmentCriteria;
                    audience.ExclusionListIds = SerializeJson(dto.Audience.ExclusionListIds);
                    audience.UpdatedAt = DateTime.UtcNow;

                    _audienceRepository.Update(audience);
                }
            }

            // Update schedule if provided
            if (dto.Schedule != null)
            {
                var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaignId);
                if (schedule != null)
                {
                    schedule.ScheduleType = dto.Schedule.ScheduleType;
                    schedule.ScheduledDateTime = dto.Schedule.ScheduledDate;
                    schedule.RecurrencePattern = dto.Schedule.RecurrenceRule;
                    schedule.TimeZoneAware = dto.Schedule.TimeZoneAware;
                    schedule.TimeZone = dto.Schedule.PreferredTimeZone;
                    schedule.UpdatedAt = DateTime.UtcNow;

                    _scheduleRepository.Update(schedule);
                }
                else
                {
                    // Create new schedule if it doesn't exist
                    var newSchedule = new CampaignSchedule
                    {
                        CampaignId = campaignId,
                        ScheduleType = dto.Schedule.ScheduleType,
                        ScheduledDateTime = dto.Schedule.ScheduledDate,
                        RecurrencePattern = dto.Schedule.RecurrenceRule,
                        TimeZoneAware = dto.Schedule.TimeZoneAware,
                        TimeZone = dto.Schedule.PreferredTimeZone
                    };
                    await _scheduleRepository.AddAsync(newSchedule);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Cannot delete running campaigns
            if (campaign.Status == CampaignStatus.Running)
            {
                _logger.LogWarning("Attempted to delete running campaign {CampaignId}", campaignId);
                return false;
            }

            // Soft delete
            campaign.IsDeleted = true;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DuplicateCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Create new campaign
            var newCampaign = new Campaign
            {
                UserId = userId,
                Name = campaign.Name + " (Copy)",
                Description = campaign.Description,
                Type = campaign.Type,
                Status = CampaignStatus.Draft
            };

            await _campaignRepository.AddAsync(newCampaign);
            await _unitOfWork.SaveChangesAsync();

            // Duplicate content
            var content = await _contentRepository.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
            if (content != null)
            {
                var newContent = new CampaignContent
                {
                    CampaignId = newCampaign.Id,
                    Channel = content.Channel,
                    Subject = content.Subject,
                    MessageBody = content.MessageBody,
                    HTMLContent = content.HTMLContent,
                    MediaUrls = content.MediaUrls,
                    MessageTemplateId = content.MessageTemplateId,
                    PersonalizationTokens = content.PersonalizationTokens
                };
                await _contentRepository.AddAsync(newContent);
            }

            // Duplicate audience
            var audience = await _audienceRepository.FirstOrDefaultAsync(a => a.CampaignId == campaignId);
            if (audience != null)
            {
                var newAudience = new CampaignAudience
                {
                    CampaignId = newCampaign.Id,
                    TargetType = audience.TargetType,
                    GroupIds = audience.GroupIds,
                    SegmentCriteria = audience.SegmentCriteria,
                    ExclusionListIds = audience.ExclusionListIds
                };
                await _audienceRepository.AddAsync(newAudience);
            }

            // Duplicate schedule
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaignId);
            if (schedule != null)
            {
                var newSchedule = new CampaignSchedule
                {
                    CampaignId = newCampaign.Id,
                    ScheduleType = schedule.ScheduleType,
                    ScheduledDateTime = schedule.ScheduledDateTime,
                    RecurrencePattern = schedule.RecurrencePattern,
                    TimeZoneAware = schedule.TimeZoneAware,
                    TimeZone = schedule.TimeZone
                };
                await _scheduleRepository.AddAsync(newSchedule);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ScheduleCampaignAsync(string userId, int campaignId, DateTime scheduledDate)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Can only schedule Draft campaigns
            if (campaign.Status != CampaignStatus.Draft)
            {
                _logger.LogWarning("Attempted to schedule non-draft campaign {CampaignId}", campaignId);
                return false;
            }

            campaign.Status = CampaignStatus.Scheduled;
            campaign.ScheduledAt = scheduledDate;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> StartCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Can only start Draft or Scheduled campaigns
            if (campaign.Status != CampaignStatus.Draft && campaign.Status != CampaignStatus.Scheduled)
            {
                _logger.LogWarning("Attempted to start campaign {CampaignId} with status {Status}", campaignId, campaign.Status);
                return false;
            }

            campaign.Status = CampaignStatus.Running;
            campaign.StartedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            // TODO: Queue Hangfire job to process campaign messages
            _logger.LogInformation("Campaign {CampaignId} started. TODO: Queue Hangfire job for message processing", campaignId);

            return true;
        }

        public async Task<bool> PauseCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Can only pause Running campaigns
            if (campaign.Status != CampaignStatus.Running)
            {
                _logger.LogWarning("Attempted to pause non-running campaign {CampaignId}", campaignId);
                return false;
            }

            campaign.Status = CampaignStatus.Paused;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResumeCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Can only resume Paused campaigns
            if (campaign.Status != CampaignStatus.Paused)
            {
                _logger.LogWarning("Attempted to resume non-paused campaign {CampaignId}", campaignId);
                return false;
            }

            campaign.Status = CampaignStatus.Running;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            // Cannot cancel already completed or failed campaigns
            if (campaign.Status == CampaignStatus.Completed || campaign.Status == CampaignStatus.Failed)
            {
                _logger.LogWarning("Attempted to cancel completed/failed campaign {CampaignId}", campaignId);
                return false;
            }

            campaign.Status = CampaignStatus.Failed;
            campaign.UpdatedAt = DateTime.UtcNow;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CampaignStatsDto> GetCampaignStatsAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found for user {UserId}", campaignId, userId);
                throw new ArgumentException($"Campaign with ID {campaignId} not found");
            }

            var messages = await _messageRepository.FindAsync(m => m.CampaignId == campaignId && !m.IsDeleted);

            var totalSent = messages.Count();
            var delivered = messages.Count(m => m.Status == MessageStatus.Delivered);
            var failed = messages.Count(m => m.Status == MessageStatus.Failed);
            var bounced = messages.Count(m => m.Status == MessageStatus.Bounced);

            var deliveryRate = totalSent > 0 ? (decimal)delivered / totalSent * 100 : 0;
            var failureRate = totalSent > 0 ? (decimal)failed / totalSent * 100 : 0;
            var estimatedCost = messages.Sum(m => m.CostAmount);

            return new CampaignStatsDto
            {
                TotalSent = totalSent,
                Delivered = delivered,
                Failed = failed,
                Bounced = bounced,
                DeliveryRate = Math.Round(deliveryRate, 2),
                FailureRate = Math.Round(failureRate, 2),
                EstimatedCost = Math.Round(estimatedCost, 2)
            };
        }

        public async Task<int> CalculateAudienceSizeAsync(string userId, CampaignAudienceDto audience)
        {
            if (audience.TargetType == TargetType.All)
            {
                // Count all active contacts for the user
                var count = await _contactRepository.CountAsync(c => c.UserId == userId && !c.IsDeleted);
                return count;
            }
            else if (audience.TargetType == TargetType.Groups && audience.GroupIds != null && audience.GroupIds.Any())
            {
                // Count distinct contacts in specified groups
                var groupMembers = await _groupMemberRepository.FindAsync(gm =>
                    audience.GroupIds.Contains(gm.ContactGroupId) && !gm.IsDeleted);

                var contactIds = groupMembers.Select(gm => gm.ContactId).Distinct().ToList();
                var count = await _contactRepository.CountAsync(c =>
                    contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);

                return count;
            }
            else if (audience.TargetType == TargetType.Segments)
            {
                // TODO: Implement segment criteria parsing
                _logger.LogWarning("Segment criteria parsing not yet implemented");
                return 0;
            }

            return 0;
        }

        private async Task<CampaignDto> MapToCampaignDtoAsync(Campaign campaign)
        {
            var dto = _mapper.Map<CampaignDto>(campaign);

            // Load content
            var content = await _contentRepository.FirstOrDefaultAsync(c => c.CampaignId == campaign.Id);
            if (content != null)
            {
                dto.Content = new CampaignContentDto
                {
                    Channel = content.Channel,
                    Subject = content.Subject,
                    MessageBody = content.MessageBody ?? string.Empty,
                    HTMLContent = content.HTMLContent,
                    MediaUrls = DeserializeJson<List<string>>(content.MediaUrls),
                    TemplateId = content.MessageTemplateId,
                    PersonalizationTokens = DeserializeJson<Dictionary<string, string>>(content.PersonalizationTokens)
                };
            }

            // Load audience
            var audience = await _audienceRepository.FirstOrDefaultAsync(a => a.CampaignId == campaign.Id);
            if (audience != null)
            {
                dto.Audience = new CampaignAudienceDto
                {
                    TargetType = audience.TargetType,
                    GroupIds = DeserializeJson<List<int>>(audience.GroupIds),
                    SegmentCriteria = audience.SegmentCriteria,
                    ExclusionListIds = DeserializeJson<List<int>>(audience.ExclusionListIds)
                };
            }

            // Load schedule
            var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.CampaignId == campaign.Id);
            if (schedule != null)
            {
                dto.Schedule = new CampaignScheduleDto
                {
                    ScheduleType = schedule.ScheduleType,
                    ScheduledDate = schedule.ScheduledDateTime,
                    RecurrenceRule = schedule.RecurrencePattern,
                    TimeZoneAware = schedule.TimeZoneAware,
                    PreferredTimeZone = schedule.TimeZone
                };
            }

            return dto;
        }

        private T? DeserializeJson<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON: {Json}", json);
                return null;
            }
        }

        private string? SerializeJson<T>(T? obj) where T : class
        {
            if (obj == null)
                return null;

            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize object to JSON");
                return null;
            }
        }
    }
}
