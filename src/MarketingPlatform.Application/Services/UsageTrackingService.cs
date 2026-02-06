using MarketingPlatform.Application.DTOs.Usage;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class UsageTrackingService : IUsageTrackingService
    {
        private readonly IRepository<UsageTracking> _usageRepository;
        private readonly IRepository<UserSubscription> _subscriptionRepository;
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<BillingHistory> _billingHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsageTrackingService> _logger;

        private const decimal SMS_OVERAGE_RATE = 0.01m;
        private const decimal MMS_OVERAGE_RATE = 0.02m;
        private const decimal EMAIL_OVERAGE_RATE = 0.001m;

        public UsageTrackingService(
            IRepository<UsageTracking> usageRepository,
            IRepository<UserSubscription> subscriptionRepository,
            IRepository<SubscriptionPlan> planRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<BillingHistory> billingHistoryRepository,
            IUnitOfWork unitOfWork,
            ILogger<UsageTrackingService> logger)
        {
            _usageRepository = usageRepository;
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _invoiceRepository = invoiceRepository;
            _billingHistoryRepository = billingHistoryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UsageStatsDto> GetCurrentUsageAsync(string userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await GetUsageForPeriodAsync(userId, new DateTime(now.Year, now.Month, 1), now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current usage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UsageStatsDto> GetUsageForPeriodAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var usage = await _usageRepository.FirstOrDefaultAsync(
                    u => u.UserId == userId && u.Year == startDate.Year && u.Month == startDate.Month);

                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                var plan = subscription != null 
                    ? await _planRepository.GetByIdAsync(subscription.SubscriptionPlanId) 
                    : null;

                var smsUsed = usage?.SMSUsed ?? 0;
                var mmsUsed = usage?.MMSUsed ?? 0;
                var emailUsed = usage?.EmailUsed ?? 0;

                var smsLimit = plan?.SMSLimit ?? 0;
                var mmsLimit = plan?.MMSLimit ?? 0;
                var emailLimit = plan?.EmailLimit ?? 0;

                var smsOverage = Math.Max(0, smsUsed - smsLimit);
                var mmsOverage = Math.Max(0, mmsUsed - mmsLimit);
                var emailOverage = Math.Max(0, emailUsed - emailLimit);

                var overageCost = (smsOverage * SMS_OVERAGE_RATE) + 
                                 (mmsOverage * MMS_OVERAGE_RATE) + 
                                 (emailOverage * EMAIL_OVERAGE_RATE);

                return new UsageStatsDto
                {
                    UserId = userId,
                    SMSCount = smsUsed,
                    MMSCount = mmsUsed,
                    EmailCount = emailUsed,
                    SMSLimit = smsLimit,
                    MMSLimit = mmsLimit,
                    EmailLimit = emailLimit,
                    SMSOverage = smsOverage,
                    MMSOverage = mmsOverage,
                    EmailOverage = emailOverage,
                    OverageCost = overageCost,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage for period for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> TrackUsageAsync(string userId, string channel, int count)
        {
            try
            {
                var now = DateTime.UtcNow;
                var usage = await _usageRepository.FirstOrDefaultAsync(
                    u => u.UserId == userId && u.Year == now.Year && u.Month == now.Month);

                if (usage == null)
                {
                    usage = new UsageTracking
                    {
                        UserId = userId,
                        Year = now.Year,
                        Month = now.Month,
                        SMSUsed = 0,
                        MMSUsed = 0,
                        EmailUsed = 0,
                        ContactsUsed = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _usageRepository.AddAsync(usage);
                }

                switch (channel.ToUpper())
                {
                    case "SMS":
                        usage.SMSUsed += count;
                        break;
                    case "MMS":
                        usage.MMSUsed += count;
                        break;
                    case "EMAIL":
                        usage.EmailUsed += count;
                        break;
                    case "CONTACT":
                    case "CONTACTS":
                        usage.ContactsUsed += count;
                        break;
                    default:
                        _logger.LogWarning("Unknown channel type: {Channel}", channel);
                        return false;
                }

                usage.UpdatedAt = DateTime.UtcNow;
                _usageRepository.Update(usage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Usage tracked for user {UserId}: {Channel} +{Count}", userId, channel, count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking usage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ResetMonthlyUsageAsync(string userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var lastMonth = now.AddMonths(-1);

                var currentUsage = await _usageRepository.FirstOrDefaultAsync(
                    u => u.UserId == userId && u.Year == now.Year && u.Month == now.Month);

                if (currentUsage != null)
                {
                    _logger.LogInformation("Usage already exists for current month for user {UserId}", userId);
                    return true;
                }

                var newUsage = new UsageTracking
                {
                    UserId = userId,
                    Year = now.Year,
                    Month = now.Month,
                    SMSUsed = 0,
                    MMSUsed = 0,
                    EmailUsed = 0,
                    ContactsUsed = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _usageRepository.AddAsync(newUsage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Monthly usage reset for user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting monthly usage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> CalculateOverageAsync(string userId)
        {
            try
            {
                var stats = await GetCurrentUsageAsync(userId);
                return stats.OverageCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OverageDetailsDto> GetOverageDetailsAsync(string userId)
        {
            try
            {
                var stats = await GetCurrentUsageAsync(userId);

                return new OverageDetailsDto
                {
                    SMSOverage = (int)stats.SMSOverage,
                    MMSOverage = (int)stats.MMSOverage,
                    EmailOverage = (int)stats.EmailOverage,
                    SMSOverageCost = stats.SMSOverage * SMS_OVERAGE_RATE,
                    MMSOverageCost = stats.MMSOverage * MMS_OVERAGE_RATE,
                    EmailOverageCost = stats.EmailOverage * EMAIL_OVERAGE_RATE,
                    TotalOverageCost = stats.OverageCost
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overage details for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UsageAlertDto>> GetUsageAlertsAsync(string userId)
        {
            try
            {
                var stats = await GetCurrentUsageAsync(userId);
                var alerts = new List<UsageAlertDto>();

                void CheckAlert(string channel, int used, int limit)
                {
                    if (limit == 0) return;

                    var percentage = (int)((used / (double)limit) * 100);
                    if (percentage >= 75)
                    {
                        alerts.Add(new UsageAlertDto
                        {
                            UsagePercentage = percentage,
                            Channel = channel,
                            Used = used,
                            Limit = limit,
                            AlertTime = DateTime.UtcNow
                        });
                    }
                }

                CheckAlert("SMS", stats.SMSCount, stats.SMSLimit);
                CheckAlert("MMS", stats.MMSCount, stats.MMSLimit);
                CheckAlert("Email", stats.EmailCount, stats.EmailLimit);

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage alerts for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckAndCreateAlertsAsync(string userId)
        {
            try
            {
                var alerts = await GetUsageAlertsAsync(userId);

                foreach (var alert in alerts)
                {
                    if (alert.UsagePercentage >= 75 && alert.UsagePercentage < 90)
                    {
                        _logger.LogInformation("Usage alert (75%): User {UserId}, Channel {Channel}", userId, alert.Channel);
                    }
                    else if (alert.UsagePercentage >= 90 && alert.UsagePercentage < 100)
                    {
                        _logger.LogWarning("Usage alert (90%): User {UserId}, Channel {Channel}", userId, alert.Channel);
                    }
                    else if (alert.UsagePercentage >= 100)
                    {
                        _logger.LogWarning("Usage alert (100%): User {UserId}, Channel {Channel}", userId, alert.Channel);
                    }
                }

                return alerts.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and creating alerts for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SetAlertThresholdAsync(string userId, string channel, int percentage)
        {
            try
            {
                if (percentage < 0 || percentage > 100)
                {
                    _logger.LogWarning("Invalid threshold percentage: {Percentage}", percentage);
                    return false;
                }

                _logger.LogInformation("Alert threshold set for user {UserId}, channel {Channel}: {Percentage}%", 
                    userId, channel, percentage);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting alert threshold for user {UserId}", userId);
                throw;
            }
        }

        public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var subscriptions = await _subscriptionRepository.FindAsync(
                    s => s.Status == SubscriptionStatus.Active && s.StartDate <= endDate);

                var subscriptionRevenue = 0m;
                foreach (var sub in subscriptions)
                {
                    var plan = await _planRepository.GetByIdAsync(sub.SubscriptionPlanId);
                    if (plan != null)
                    {
                        subscriptionRevenue += sub.IsYearly ? plan.PriceYearly : plan.PriceMonthly;
                    }
                }

                var invoices = await _invoiceRepository.FindAsync(
                    i => i.Status == InvoiceStatus.Paid && 
                         i.PaidDate >= startDate && 
                         i.PaidDate <= endDate);

                var overageRevenue = invoices
                    .Where(i => i.Description != null && i.Description.Contains("overage", StringComparison.OrdinalIgnoreCase))
                    .Sum(i => i.Total);

                var totalRevenue = subscriptionRevenue + overageRevenue;

                var mrr = await GetMRRAsync();
                var arr = await GetARRAsync();

                var revenueByPlan = await GetRevenueByPlanAsync(startDate, endDate);

                return new RevenueAnalyticsDto
                {
                    TotalRevenue = totalRevenue,
                    SubscriptionRevenue = subscriptionRevenue,
                    OverageRevenue = overageRevenue,
                    MRR = mrr,
                    ARR = arr,
                    RevenueByPlan = revenueByPlan,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics");
                throw;
            }
        }

        public async Task<decimal> GetMRRAsync()
        {
            try
            {
                var activeSubscriptions = await _subscriptionRepository.FindAsync(
                    s => s.Status == SubscriptionStatus.Active);

                var mrr = 0m;
                foreach (var sub in activeSubscriptions)
                {
                    var plan = await _planRepository.GetByIdAsync(sub.SubscriptionPlanId);
                    if (plan != null)
                    {
                        mrr += sub.IsYearly ? plan.PriceYearly / 12 : plan.PriceMonthly;
                    }
                }

                return mrr;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating MRR");
                throw;
            }
        }

        public async Task<decimal> GetARRAsync()
        {
            try
            {
                var mrr = await GetMRRAsync();
                return mrr * 12;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ARR");
                throw;
            }
        }

        public async Task<List<RevenueByPlanDto>> GetRevenueByPlanAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var activeSubscriptions = await _subscriptionRepository.FindAsync(
                    s => s.Status == SubscriptionStatus.Active && s.StartDate <= endDate);

                var planGroups = activeSubscriptions.GroupBy(s => s.SubscriptionPlanId);

                var result = new List<RevenueByPlanDto>();

                foreach (var group in planGroups)
                {
                    var plan = await _planRepository.GetByIdAsync(group.Key);
                    if (plan != null)
                    {
                        var revenue = 0m;
                        foreach (var sub in group)
                        {
                            revenue += sub.IsYearly ? plan.PriceYearly : plan.PriceMonthly;
                        }

                        result.Add(new RevenueByPlanDto
                        {
                            PlanId = plan.Id,
                            PlanName = plan.Name,
                            SubscriberCount = group.Count(),
                            Revenue = revenue
                        });
                    }
                }

                return result.OrderByDescending(r => r.Revenue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue by plan");
                throw;
            }
        }
    }
}
