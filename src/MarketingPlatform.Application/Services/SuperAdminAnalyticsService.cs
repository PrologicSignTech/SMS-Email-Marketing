using MarketingPlatform.Application.DTOs.Analytics;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class SuperAdminAnalyticsService : ISuperAdminAnalyticsService
    {
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<UserSubscription> _subscriptionRepository;
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<BillingHistory> _billingHistoryRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IRepository<MessageDeliveryAttempt> _deliveryAttemptRepository;
        private readonly IRepository<ProviderLog> _providerLogRepository;
        private readonly IRepository<UsageTracking> _usageTrackingRepository;
        private readonly ILogger<SuperAdminAnalyticsService> _logger;

        public SuperAdminAnalyticsService(
            IRepository<ApplicationUser> userRepository,
            IRepository<UserSubscription> subscriptionRepository,
            IRepository<SubscriptionPlan> planRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<BillingHistory> billingHistoryRepository,
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignMessage> messageRepository,
            IRepository<MessageDeliveryAttempt> deliveryAttemptRepository,
            IRepository<ProviderLog> providerLogRepository,
            IRepository<UsageTracking> usageTrackingRepository,
            ILogger<SuperAdminAnalyticsService> logger)
        {
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _invoiceRepository = invoiceRepository;
            _billingHistoryRepository = billingHistoryRepository;
            _campaignRepository = campaignRepository;
            _messageRepository = messageRepository;
            _deliveryAttemptRepository = deliveryAttemptRepository;
            _providerLogRepository = providerLogRepository;
            _usageTrackingRepository = usageTrackingRepository;
            _logger = logger;
        }

        public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting platform analytics");

                var totalUsers = await _userRepository.CountAsync();
                
                var activeSubscriptions = await _subscriptionRepository.CountAsync(s => 
                    s.Status == SubscriptionStatus.Active && !s.IsDeleted);
                
                var trialUsers = await _subscriptionRepository.CountAsync(s => 
                    s.Status == SubscriptionStatus.Trial && !s.IsDeleted);
                
                var canceledSubscriptions = await _subscriptionRepository.CountAsync(s => 
                    s.Status == SubscriptionStatus.Canceled && !s.IsDeleted);

                var paidInvoices = await _invoiceRepository
                    .GetQueryable()
                    .Where(i => i.Status == InvoiceStatus.Paid && !i.IsDeleted)
                    .ToListAsync();
                
                var totalRevenue = paidInvoices.Sum(i => i.Total);

                var mrr = await CalculateMRRAsync();
                var arpu = await CalculateARPUAsync();
                var churnRate = await CalculateChurnRateAsync(1);

                var subscriptionsByPlan = await GetSubscriptionDistributionAsync();
                var usageByChannel = await GetUsageByChannelAsync();

                return new PlatformAnalyticsDto
                {
                    TotalUsers = totalUsers,
                    ActiveSubscriptions = activeSubscriptions,
                    TrialUsers = trialUsers,
                    CanceledSubscriptions = canceledSubscriptions,
                    TotalRevenue = totalRevenue,
                    MRR = mrr,
                    ARPU = arpu,
                    ChurnRate = churnRate,
                    SubscriptionsByPlan = subscriptionsByPlan,
                    UsageByChannel = usageByChannel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform analytics");
                throw;
            }
        }

        public async Task<List<UserGrowthDto>> GetUserGrowthAsync(int months)
        {
            try
            {
                _logger.LogInformation("Getting user growth for {Months} months", months);

                var startDate = DateTime.UtcNow.AddMonths(-months).Date;
                
                var users = await _userRepository
                    .GetQueryable()
                    .Select(u => u.CreatedAt)
                    .ToListAsync();

                var growthData = new List<UserGrowthDto>();
                
                for (int i = months - 1; i >= 0; i--)
                {
                    var monthDate = DateTime.UtcNow.AddMonths(-i);
                    var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    var newUsers = users.Count(u => u >= monthStart && u < monthEnd);
                    var totalUsers = users.Count(u => u < monthEnd);

                    growthData.Add(new UserGrowthDto
                    {
                        Year = monthDate.Year,
                        Month = monthDate.Month,
                        MonthName = monthDate.ToString("MMMM"),
                        NewUsers = newUsers,
                        TotalUsers = totalUsers
                    });
                }

                return growthData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user growth");
                throw;
            }
        }

        public async Task<List<SubscriptionsByPlanDto>> GetSubscriptionDistributionAsync()
        {
            try
            {
                _logger.LogInformation("Getting subscription distribution");

                var activeSubscriptions = await _subscriptionRepository
                    .GetQueryable()
                    .Where(s => (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial) 
                        && !s.IsDeleted)
                    .Include(s => s.SubscriptionPlan)
                    .ToListAsync();

                var totalCount = activeSubscriptions.Count;
                
                if (totalCount == 0)
                {
                    return new List<SubscriptionsByPlanDto>();
                }

                var distribution = activeSubscriptions
                    .GroupBy(s => new { s.SubscriptionPlanId, s.SubscriptionPlan.Name })
                    .Select(g => new SubscriptionsByPlanDto
                    {
                        PlanId = g.Key.SubscriptionPlanId,
                        PlanName = g.Key.Name,
                        Count = g.Count(),
                        Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 2)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();

                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription distribution");
                throw;
            }
        }

        public async Task<BillingAnalyticsDto> GetBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting billing analytics from {StartDate} to {EndDate}", startDate, endDate);

                var mrr = await CalculateMRRAsync();
                var arr = mrr * 12;
                var churnRate = await CalculateChurnRateAsync(1);
                var arpu = await CalculateARPUAsync();
                
                var averageLTV = churnRate > 0 ? arpu / (churnRate / 100) : 0;

                var monthlyRevenue = await GetMonthlyRevenueAsync(12);
                var churnByPlan = await GetChurnByPlanAsync();

                return new BillingAnalyticsDto
                {
                    MRR = mrr,
                    ARR = arr,
                    ChurnRate = churnRate,
                    ARPU = arpu,
                    AverageLTV = averageLTV,
                    MonthlyRevenue = monthlyRevenue,
                    ChurnByPlan = churnByPlan
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing analytics");
                throw;
            }
        }

        public async Task<decimal> CalculateChurnRateAsync(int months = 1)
        {
            try
            {
                _logger.LogInformation("Calculating churn rate for {Months} months", months);

                var periodStart = DateTime.UtcNow.AddMonths(-months).Date;
                
                var activeAtStart = await _subscriptionRepository.CountAsync(s =>
                    s.StartDate < periodStart &&
                    (s.EndDate == null || s.EndDate >= periodStart) &&
                    s.Status == SubscriptionStatus.Active &&
                    !s.IsDeleted);

                if (activeAtStart == 0)
                {
                    return 0;
                }

                var canceledInPeriod = await _subscriptionRepository.CountAsync(s =>
                    s.CanceledAt >= periodStart &&
                    s.CanceledAt <= DateTime.UtcNow &&
                    s.Status == SubscriptionStatus.Canceled &&
                    !s.IsDeleted);

                var churnRate = (decimal)canceledInPeriod / activeAtStart * 100;
                return Math.Round(churnRate, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating churn rate");
                throw;
            }
        }

        public async Task<decimal> CalculateARPUAsync()
        {
            try
            {
                _logger.LogInformation("Calculating ARPU");

                var activeUsers = await _subscriptionRepository.CountAsync(s =>
                    s.Status == SubscriptionStatus.Active && !s.IsDeleted);

                if (activeUsers == 0)
                {
                    return 0;
                }

                var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var nextMonth = currentMonth.AddMonths(1);

                var monthlyRevenue = await _invoiceRepository
                    .GetQueryable()
                    .Where(i => i.Status == InvoiceStatus.Paid &&
                        i.InvoiceDate >= currentMonth &&
                        i.InvoiceDate < nextMonth &&
                        !i.IsDeleted)
                    .SumAsync(i => i.Total);

                var arpu = monthlyRevenue / activeUsers;
                return Math.Round(arpu, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ARPU");
                throw;
            }
        }

        public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months)
        {
            try
            {
                _logger.LogInformation("Getting monthly revenue for {Months} months", months);

                var monthlyRevenue = new List<MonthlyRevenueDto>();

                for (int i = months - 1; i >= 0; i--)
                {
                    var monthDate = DateTime.UtcNow.AddMonths(-i);
                    var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    var revenue = await _invoiceRepository
                        .GetQueryable()
                        .Where(i => i.Status == InvoiceStatus.Paid &&
                            i.InvoiceDate >= monthStart &&
                            i.InvoiceDate < monthEnd &&
                            !i.IsDeleted)
                        .SumAsync(i => i.Total);

                    var newSubscriptions = await _subscriptionRepository.CountAsync(s =>
                        s.StartDate >= monthStart &&
                        s.StartDate < monthEnd &&
                        !s.IsDeleted);

                    var canceledSubscriptions = await _subscriptionRepository.CountAsync(s =>
                        s.CanceledAt >= monthStart &&
                        s.CanceledAt < monthEnd &&
                        !s.IsDeleted);

                    monthlyRevenue.Add(new MonthlyRevenueDto
                    {
                        Year = monthDate.Year,
                        Month = monthDate.Month,
                        MonthName = monthDate.ToString("MMMM"),
                        Revenue = revenue,
                        NewSubscriptions = newSubscriptions,
                        CanceledSubscriptions = canceledSubscriptions
                    });
                }

                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue");
                throw;
            }
        }

        public async Task<SystemHealthDto> GetSystemHealthAsync()
        {
            try
            {
                _logger.LogInformation("Getting system health");

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var totalCampaigns = await _campaignRepository.CountAsync(c => !c.IsDeleted);
                
                var activeCampaigns = await _campaignRepository.CountAsync(c => 
                    c.Status == CampaignStatus.Running && !c.IsDeleted);

                var messagesSentToday = await _messageRepository.CountAsync(m =>
                    m.SentAt >= today &&
                    m.SentAt < tomorrow &&
                    !m.IsDeleted);

                var failedMessagesToday = await _messageRepository.CountAsync(m =>
                    m.Status == MessageStatus.Failed &&
                    m.FailedAt >= today &&
                    m.FailedAt < tomorrow &&
                    !m.IsDeleted);

                var successRate = messagesSentToday > 0
                    ? Math.Round((decimal)(messagesSentToday - failedMessagesToday) / messagesSentToday * 100, 2)
                    : 100;

                var providerHealth = await GetProviderHealthAsync();
                
                var isHealthy = successRate >= 95 && providerHealth.All(p => p.SuccessRate >= 90);

                return new SystemHealthDto
                {
                    IsHealthy = isHealthy,
                    TotalCampaigns = totalCampaigns,
                    ActiveCampaigns = activeCampaigns,
                    MessagesSentToday = messagesSentToday,
                    FailedMessagesToday = failedMessagesToday,
                    MessageSuccessRate = successRate,
                    ProviderHealth = providerHealth,
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                throw;
            }
        }

        public async Task<List<ProviderHealthDto>> GetProviderHealthAsync()
        {
            try
            {
                _logger.LogInformation("Getting provider health");

                var last24Hours = DateTime.UtcNow.AddHours(-24);

                var deliveryAttempts = await _deliveryAttemptRepository
                    .GetQueryable()
                    .Where(d => d.AttemptedAt >= last24Hours && !d.IsDeleted)
                    .ToListAsync();

                var providerGroups = deliveryAttempts
                    .Where(d => !string.IsNullOrEmpty(d.ProviderName))
                    .GroupBy(d => d.ProviderName!)
                    .Select(g =>
                    {
                        var successCount = g.Count(d => d.Success);
                        var failureCount = g.Count(d => !d.Success);
                        var totalCount = g.Count();
                        var successRate = totalCount > 0
                            ? Math.Round((decimal)successCount / totalCount * 100, 2)
                            : 0;

                        return new ProviderHealthDto
                        {
                            ProviderName = g.Key,
                            IsHealthy = successRate >= 90,
                            SuccessCount = successCount,
                            FailureCount = failureCount,
                            SuccessRate = successRate,
                            LastSuccess = g.Where(d => d.Success).Max(d => (DateTime?)d.AttemptedAt),
                            LastFailure = g.Where(d => !d.Success).Max(d => (DateTime?)d.AttemptedAt)
                        };
                    })
                    .OrderByDescending(p => p.SuccessRate)
                    .ToList();

                return providerGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider health");
                throw;
            }
        }

        public async Task<MessageDeliveryStatsDto> GetMessageDeliveryStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30).Date;
                var end = endDate ?? DateTime.UtcNow.Date.AddDays(1);

                _logger.LogInformation("Getting message delivery stats from {StartDate} to {EndDate}", start, end);

                var messages = await _messageRepository
                    .GetQueryable()
                    .Where(m => m.SentAt >= start && m.SentAt < end && !m.IsDeleted)
                    .ToListAsync();

                var totalSent = messages.Count;
                var delivered = messages.Count(m => m.Status == MessageStatus.Delivered);
                var failed = messages.Count(m => m.Status == MessageStatus.Failed || m.Status == MessageStatus.Bounced);

                var deliveryRate = totalSent > 0
                    ? Math.Round((decimal)delivered / totalSent * 100, 2)
                    : 0;

                return new MessageDeliveryStatsDto
                {
                    TotalSent = totalSent,
                    Delivered = delivered,
                    Failed = failed,
                    DeliveryRate = deliveryRate,
                    PeriodStart = start,
                    PeriodEnd = end
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message delivery stats");
                throw;
            }
        }

        private async Task<decimal> CalculateMRRAsync()
        {
            var activeSubscriptions = await _subscriptionRepository
                .GetQueryable()
                .Where(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted)
                .Include(s => s.SubscriptionPlan)
                .ToListAsync();

            decimal mrr = 0;

            foreach (var subscription in activeSubscriptions)
            {
                if (subscription.SubscriptionPlan == null)
                    continue;

                if (subscription.IsYearly)
                {
                    mrr += subscription.SubscriptionPlan.PriceYearly / 12;
                }
                else
                {
                    mrr += subscription.SubscriptionPlan.PriceMonthly;
                }
            }

            return Math.Round(mrr, 2);
        }

        private async Task<List<UsageByChannelDto>> GetUsageByChannelAsync()
        {
            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var usage = await _usageTrackingRepository
                .GetQueryable()
                .Where(u => u.Year == currentMonth.Year && u.Month == currentMonth.Month && !u.IsDeleted)
                .ToListAsync();

            var totalUsers = await _subscriptionRepository.CountAsync(s =>
                s.Status == SubscriptionStatus.Active && !s.IsDeleted);

            if (totalUsers == 0)
                totalUsers = 1;

            var channels = new List<UsageByChannelDto>
            {
                new UsageByChannelDto
                {
                    Channel = "SMS",
                    TotalUsage = usage.Sum(u => u.SMSUsed),
                    AveragePerUser = Math.Round((decimal)usage.Sum(u => u.SMSUsed) / totalUsers, 2)
                },
                new UsageByChannelDto
                {
                    Channel = "MMS",
                    TotalUsage = usage.Sum(u => u.MMSUsed),
                    AveragePerUser = Math.Round((decimal)usage.Sum(u => u.MMSUsed) / totalUsers, 2)
                },
                new UsageByChannelDto
                {
                    Channel = "Email",
                    TotalUsage = usage.Sum(u => u.EmailUsed),
                    AveragePerUser = Math.Round((decimal)usage.Sum(u => u.EmailUsed) / totalUsers, 2)
                }
            };

            return channels;
        }

        private async Task<List<ChurnByPlanDto>> GetChurnByPlanAsync()
        {
            var last30Days = DateTime.UtcNow.AddDays(-30);

            var subscriptions = await _subscriptionRepository
                .GetQueryable()
                .Where(s => !s.IsDeleted)
                .Include(s => s.SubscriptionPlan)
                .ToListAsync();

            var churnByPlan = subscriptions
                .GroupBy(s => new { s.SubscriptionPlanId, s.SubscriptionPlan.Name })
                .Select(g =>
                {
                    var total = g.Count();
                    var churned = g.Count(s => s.Status == SubscriptionStatus.Canceled && 
                        s.CanceledAt >= last30Days);
                    var churnRate = total > 0 ? Math.Round((decimal)churned / total * 100, 2) : 0;

                    return new ChurnByPlanDto
                    {
                        PlanId = g.Key.SubscriptionPlanId,
                        PlanName = g.Key.Name,
                        TotalSubscribers = total,
                        Churned = churned,
                        ChurnRate = churnRate
                    };
                })
                .OrderByDescending(c => c.ChurnRate)
                .ToList();

            return churnByPlan;
        }
    }
}
