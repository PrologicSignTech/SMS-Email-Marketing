using MarketingPlatform.Application.DTOs.Analytics;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignAnalytics> _campaignAnalyticsRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactEngagement> _contactEngagementRepository;
        private readonly IRepository<CampaignMessage> _campaignMessageRepository;
        private readonly IRepository<URLShortener> _urlShortenerRepository;
        private readonly IRepository<URLClick> _urlClickRepository;

        public AnalyticsService(
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignAnalytics> campaignAnalyticsRepository,
            IRepository<Contact> contactRepository,
            IRepository<ContactEngagement> contactEngagementRepository,
            IRepository<CampaignMessage> campaignMessageRepository,
            IRepository<URLShortener> urlShortenerRepository,
            IRepository<URLClick> urlClickRepository)
        {
            _campaignRepository = campaignRepository;
            _campaignAnalyticsRepository = campaignAnalyticsRepository;
            _contactRepository = contactRepository;
            _contactEngagementRepository = contactEngagementRepository;
            _campaignMessageRepository = campaignMessageRepository;
            _urlShortenerRepository = urlShortenerRepository;
            _urlClickRepository = urlClickRepository;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId, ReportFilterDto? filter = null)
        {
            var query = _campaignRepository.GetQueryable().Where(c => c.UserId == userId && !c.IsDeleted);

            if (filter?.StartDate.HasValue == true)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter?.EndDate.HasValue == true)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            var campaigns = await query
                .Include(c => c.Analytics)
                .ToListAsync();

            var contacts = await _contactRepository.GetQueryable()
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Engagement)
                .ToListAsync();

            var totalSent = campaigns.Sum(c => c.Analytics?.TotalSent ?? 0);
            var totalDelivered = campaigns.Sum(c => c.Analytics?.TotalDelivered ?? 0);
            var totalFailed = campaigns.Sum(c => c.Analytics?.TotalFailed ?? 0);
            var totalClicks = campaigns.Sum(c => c.Analytics?.TotalClicks ?? 0);
            var totalOptOuts = campaigns.Sum(c => c.Analytics?.TotalOptOuts ?? 0);

            var recentCampaigns = campaigns
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => MapToCampaignPerformance(c))
                .ToList();

            var topPerformers = campaigns
                .Where(c => c.Status == CampaignStatus.Completed && c.Analytics != null)
                .OrderByDescending(c => c.Analytics!.DeliveryRate * 0.4m + c.Analytics.ClickRate * 0.6m)
                .Take(5)
                .Select(c => new TopPerformerDto
                {
                    CampaignId = c.Id,
                    CampaignName = c.Name,
                    PerformanceScore = c.Analytics!.DeliveryRate * 0.4m + c.Analytics.ClickRate * 0.6m,
                    DeliveryRate = c.Analytics.DeliveryRate,
                    ClickRate = c.Analytics.ClickRate
                })
                .ToList();

            return new DashboardSummaryDto
            {
                TotalCampaigns = campaigns.Count,
                ActiveCampaigns = campaigns.Count(c => c.Status == CampaignStatus.Running),
                CompletedCampaigns = campaigns.Count(c => c.Status == CampaignStatus.Completed),
                ScheduledCampaigns = campaigns.Count(c => c.Status == CampaignStatus.Scheduled),
                TotalMessagesSent = totalSent,
                TotalMessagesDelivered = totalDelivered,
                TotalMessagesFailed = totalFailed,
                OverallDeliveryRate = totalSent > 0 ? (decimal)totalDelivered / totalSent * 100 : 0,
                TotalClicks = totalClicks,
                OverallClickRate = totalDelivered > 0 ? (decimal)totalClicks / totalDelivered * 100 : 0,
                TotalOptOuts = totalOptOuts,
                OverallOptOutRate = totalDelivered > 0 ? (decimal)totalOptOuts / totalDelivered * 100 : 0,
                TotalContacts = contacts.Count,
                ActiveContacts = contacts.Count(c => c.IsActive),
                EngagedContacts = contacts.Count(c => c.Engagement != null && c.Engagement.EngagementScore > 0),
                TotalSpent = 0, // TODO: Calculate from billing/usage data
                AverageCostPerMessage = 0, // TODO: Calculate from billing/usage data
                RecentCampaigns = recentCampaigns,
                TopPerformingCampaigns = topPerformers
            };
        }

        public async Task<List<CampaignPerformanceDto>> GetCampaignPerformanceAsync(string userId, ReportFilterDto filter)
        {
            var query = _campaignRepository.GetQueryable()
                .Where(c => c.UserId == userId && !c.IsDeleted);

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(c => c.Id == filter.CampaignId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (Enum.TryParse<CampaignStatus>(filter.Status, true, out var status))
                    query = query.Where(c => c.Status == status);
            }

            var campaigns = await query.Include(c => c.Analytics).ToListAsync();
            return campaigns.Select(c => MapToCampaignPerformance(c)).ToList();
        }

        public async Task<CampaignPerformanceDto?> GetCampaignPerformanceByIdAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.GetQueryable()
                .Where(c => c.Id == campaignId && c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Analytics)
                .FirstOrDefaultAsync();

            return campaign != null ? MapToCampaignPerformance(campaign) : null;
        }

        public async Task<List<ContactEngagementHistoryDto>> GetContactEngagementHistoryAsync(string userId, ReportFilterDto filter)
        {
            var query = _contactRepository.GetQueryable()
                .Where(c => c.UserId == userId && !c.IsDeleted);

            if (filter.ContactId.HasValue)
                query = query.Where(c => c.Id == filter.ContactId.Value);

            var contacts = await query.Include(c => c.Engagement).ToListAsync();
            var contactIds = contacts.Select(c => c.Id).ToList();

            // Get campaign messages for these contacts
            var campaignMessages = await _campaignMessageRepository.GetQueryable()
                .Where(cm => contactIds.Contains(cm.ContactId))
                .Include(cm => cm.Campaign)
                .ToListAsync();

            var result = new List<ContactEngagementHistoryDto>();

            foreach (var contact in contacts)
            {
                var contactMessages = campaignMessages.Where(cm => cm.ContactId == contact.Id).ToList();
                
                var campaignHistory = contactMessages
                    .GroupBy(cm => cm.CampaignId)
                    .Select(g => new CampaignParticipationDto
                    {
                        CampaignId = g.Key,
                        CampaignName = g.First().Campaign?.Name ?? "Unknown",
                        ParticipatedAt = g.Min(cm => cm.CreatedAt),
                        MessagesReceived = g.Count(),
                        Clicks = 0, // TODO: Get from URL clicks
                        OptedOut = false // TODO: Determine from message status
                    })
                    .ToList();

                var engagementEvents = contactMessages
                    .Select(cm => new EngagementEventDto
                    {
                        EventDate = cm.CreatedAt,
                        EventType = "MessageSent",
                        CampaignName = cm.Campaign?.Name,
                        Details = $"Message sent via {cm.Campaign?.Type}"
                    })
                    .OrderByDescending(e => e.EventDate)
                    .ToList();

                result.Add(new ContactEngagementHistoryDto
                {
                    ContactId = contact.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    TotalMessagesSent = contact.Engagement?.TotalMessagesSent ?? 0,
                    TotalMessagesDelivered = contact.Engagement?.TotalMessagesDelivered ?? 0,
                    TotalClicks = contact.Engagement?.TotalClicks ?? 0,
                    LastEngagementDate = contact.Engagement?.LastEngagementDate,
                    EngagementScore = contact.Engagement?.EngagementScore ?? 0,
                    CampaignsParticipated = campaignHistory.Count,
                    CampaignHistory = campaignHistory,
                    EngagementEvents = engagementEvents
                });
            }

            return result;
        }

        public async Task<ContactEngagementHistoryDto?> GetContactEngagementByIdAsync(string userId, int contactId, ReportFilterDto? filter = null)
        {
            var filterDto = filter ?? new ReportFilterDto { ContactId = contactId };
            filterDto.ContactId = contactId;

            var result = await GetContactEngagementHistoryAsync(userId, filterDto);
            return result.FirstOrDefault();
        }

        public async Task<ConversionTrackingDto?> GetConversionTrackingAsync(string userId, int campaignId, ReportFilterDto? filter = null)
        {
            var campaign = await _campaignRepository.GetQueryable()
                .Where(c => c.Id == campaignId && c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Analytics)
                .Include(c => c.URLShorteners)
                    .ThenInclude(u => u.Clicks)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return null;

            var urlConversions = campaign.URLShorteners.Select(url =>
            {
                var clicks = url.Clicks;
                if (filter?.StartDate.HasValue == true)
                    clicks = clicks.Where(c => c.ClickedAt >= filter.StartDate.Value).ToList();
                if (filter?.EndDate.HasValue == true)
                    clicks = clicks.Where(c => c.ClickedAt <= filter.EndDate.Value).ToList();

                return new UrlConversionDto
                {
                    ShortCode = url.ShortCode,
                    OriginalUrl = url.OriginalUrl,
                    TotalClicks = clicks.Count,
                    UniqueClicks = clicks.Select(c => c.IpAddress).Distinct().Count(),
                    FirstClickedAt = clicks.Any() ? clicks.Min(c => c.ClickedAt) : null,
                    LastClickedAt = clicks.Any() ? clicks.Max(c => c.ClickedAt) : null
                };
            }).ToList();

            var totalClicks = urlConversions.Sum(u => u.TotalClicks);
            var totalRecipients = campaign.TotalRecipients;
            var totalDelivered = campaign.Analytics?.TotalDelivered ?? 0;

            // Group clicks by date for timeline
            var allClicks = campaign.URLShorteners.SelectMany(u => u.Clicks);
            if (filter?.StartDate.HasValue == true)
                allClicks = allClicks.Where(c => c.ClickedAt >= filter.StartDate.Value);
            if (filter?.EndDate.HasValue == true)
                allClicks = allClicks.Where(c => c.ClickedAt <= filter.EndDate.Value);

            var timeline = allClicks
                .GroupBy(c => c.ClickedAt.Date)
                .Select(g => new ConversionTimelineDto
                {
                    Date = g.Key,
                    Clicks = g.Count(),
                    Conversions = g.Count(), // Simplified: treating clicks as conversions
                    ConversionRate = 100 // Simplified
                })
                .OrderBy(t => t.Date)
                .ToList();

            return new ConversionTrackingDto
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                TotalRecipients = totalRecipients,
                TotalClicks = totalClicks,
                TotalConversions = totalClicks, // Simplified: treating clicks as conversions
                ClickThroughRate = totalDelivered > 0 ? (decimal)totalClicks / totalDelivered * 100 : 0,
                ConversionRate = totalRecipients > 0 ? (decimal)totalClicks / totalRecipients * 100 : 0,
                ClickToConversionRate = 100, // Simplified
                UrlConversions = urlConversions,
                ConversionTimeline = timeline
            };
        }

        public async Task<List<ConversionTrackingDto>> GetConversionTrackingForCampaignsAsync(string userId, ReportFilterDto filter)
        {
            var query = _campaignRepository.GetQueryable()
                .Where(c => c.UserId == userId && !c.IsDeleted);

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(c => c.Id == filter.CampaignId.Value);

            var campaigns = await query.ToListAsync();
            var result = new List<ConversionTrackingDto>();

            foreach (var campaign in campaigns)
            {
                var tracking = await GetConversionTrackingAsync(userId, campaign.Id, filter);
                if (tracking != null)
                    result.Add(tracking);
            }

            return result;
        }

        private CampaignPerformanceDto MapToCampaignPerformance(Campaign campaign)
        {
            var duration = campaign.CompletedAt.HasValue && campaign.StartedAt.HasValue
                ? campaign.CompletedAt.Value - campaign.StartedAt.Value
                : (TimeSpan?)null;

            var totalSent = campaign.Analytics?.TotalSent ?? 0;
            var estimatedCost = totalSent * 0.01m; // Simplified: $0.01 per message

            return new CampaignPerformanceDto
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                CampaignType = campaign.Type.ToString(),
                Status = campaign.Status.ToString(),
                StartedAt = campaign.StartedAt,
                CompletedAt = campaign.CompletedAt,
                TotalSent = campaign.Analytics?.TotalSent ?? 0,
                TotalDelivered = campaign.Analytics?.TotalDelivered ?? 0,
                TotalFailed = campaign.Analytics?.TotalFailed ?? 0,
                TotalClicks = campaign.Analytics?.TotalClicks ?? 0,
                TotalOptOuts = campaign.Analytics?.TotalOptOuts ?? 0,
                DeliveryRate = campaign.Analytics?.DeliveryRate ?? 0,
                ClickRate = campaign.Analytics?.ClickRate ?? 0,
                OptOutRate = campaign.Analytics?.OptOutRate ?? 0,
                EngagementRate = campaign.Analytics?.ClickRate ?? 0,
                EstimatedCost = estimatedCost,
                CostPerMessage = totalSent > 0 ? estimatedCost / totalSent : 0,
                CostPerClick = campaign.Analytics?.TotalClicks > 0 ? estimatedCost / campaign.Analytics.TotalClicks : 0,
                Duration = duration,
                AverageDeliveryTime = null // TODO: Calculate from message delivery attempts
            };
        }
    }
}
