using AutoMapper;
using MarketingPlatform.Application.DTOs.Billing;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IRepository<UserSubscription> _subscriptionRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<BillingHistory> _billingHistoryRepository;
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IStripeService _stripeService;
        private readonly IPayPalService _payPalService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            IRepository<UserSubscription> subscriptionRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<BillingHistory> billingHistoryRepository,
            IRepository<SubscriptionPlan> planRepository,
            IStripeService stripeService,
            IPayPalService payPalService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BillingService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _invoiceRepository = invoiceRepository;
            _billingHistoryRepository = billingHistoryRepository;
            _planRepository = planRepository;
            _stripeService = stripeService;
            _payPalService = payPalService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserSubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto)
        {
            try
            {
                var plan = await _planRepository.GetByIdAsync(dto.PlanId);
                if (plan == null)
                {
                    _logger.LogError("Subscription plan not found: {PlanId}", dto.PlanId);
                    throw new InvalidOperationException($"Subscription plan {dto.PlanId} not found");
                }

                var existingSubscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == dto.UserId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                if (existingSubscription != null)
                {
                    _logger.LogWarning("User already has an active subscription: {UserId}", dto.UserId);
                    throw new InvalidOperationException("User already has an active subscription");
                }

                var subscription = new UserSubscription
                {
                    UserId = dto.UserId,
                    SubscriptionPlanId = dto.PlanId,
                    PaymentProvider = dto.PaymentProvider,
                    IsYearly = dto.IsYearly,
                    StartDate = DateTime.UtcNow,
                    Status = dto.StartTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (dto.StartTrial)
                {
                    subscription.TrialEndDate = DateTime.UtcNow.AddDays(dto.TrialDays);
                }

                await _subscriptionRepository.AddAsync(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription created for user {UserId} with plan {PlanId}", dto.UserId, dto.PlanId);

                return await MapToSubscriptionDto(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for user {UserId}", dto.UserId);
                throw;
            }
        }

        public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(string userId)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                if (subscription == null)
                    return null;

                return await MapToSubscriptionDto(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserSubscriptionDto?> UpgradeSubscriptionAsync(string userId, SubscriptionUpgradeDto dto)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return null;
                }

                var newPlan = await _planRepository.GetByIdAsync(dto.NewPlanId);
                if (newPlan == null)
                {
                    _logger.LogError("New subscription plan not found: {PlanId}", dto.NewPlanId);
                    throw new InvalidOperationException($"Subscription plan {dto.NewPlanId} not found");
                }

                var oldPlan = await _planRepository.GetByIdAsync(subscription.SubscriptionPlanId);
                var oldPrice = subscription.IsYearly ? oldPlan!.PriceYearly : oldPlan!.PriceMonthly;
                var newPrice = subscription.IsYearly ? newPlan.PriceYearly : newPlan.PriceMonthly;

                if (dto.Prorated && newPrice > oldPrice)
                {
                    var daysRemaining = (subscription.EndDate ?? DateTime.UtcNow.AddMonths(subscription.IsYearly ? 12 : 1)).Subtract(DateTime.UtcNow).Days;
                    var totalDays = subscription.IsYearly ? 365 : 30;
                    var proratedAmount = (newPrice - oldPrice) * daysRemaining / totalDays;

                    if (proratedAmount > 0)
                    {
                        await CreateInvoiceAsync(userId, proratedAmount, "Prorated upgrade charge", subscription.PaymentProvider);
                    }
                }

                subscription.SubscriptionPlanId = dto.NewPlanId;
                subscription.UpdatedAt = DateTime.UtcNow;

                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription upgraded for user {UserId} to plan {PlanId}", userId, dto.NewPlanId);

                return await MapToSubscriptionDto(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserSubscriptionDto?> DowngradeSubscriptionAsync(string userId, int newPlanId)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return null;
                }

                var newPlan = await _planRepository.GetByIdAsync(newPlanId);
                if (newPlan == null)
                {
                    _logger.LogError("New subscription plan not found: {PlanId}", newPlanId);
                    throw new InvalidOperationException($"Subscription plan {newPlanId} not found");
                }

                subscription.SubscriptionPlanId = newPlanId;
                subscription.UpdatedAt = DateTime.UtcNow;

                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription downgraded for user {UserId} to plan {PlanId}", userId, newPlanId);

                return await MapToSubscriptionDto(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downgrading subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(string userId, string reason)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return false;
                }

                subscription.Status = SubscriptionStatus.Canceled;
                subscription.CanceledAt = DateTime.UtcNow;
                subscription.CancellationReason = reason;
                subscription.EndDate = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;

                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription canceled for user {UserId}. Reason: {Reason}", userId, reason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ReactivateSubscriptionAsync(string userId)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && s.Status == SubscriptionStatus.Canceled);

                if (subscription == null)
                {
                    _logger.LogWarning("No canceled subscription found for user {UserId}", userId);
                    return false;
                }

                subscription.Status = SubscriptionStatus.Active;
                subscription.CanceledAt = null;
                subscription.CancellationReason = null;
                subscription.EndDate = null;
                subscription.UpdatedAt = DateTime.UtcNow;

                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription reactivated for user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RenewSubscriptionAsync(string userId)
        {
            try
            {
                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return false;
                }

                var plan = await _planRepository.GetByIdAsync(subscription.SubscriptionPlanId);
                if (plan == null)
                {
                    _logger.LogError("Subscription plan not found: {PlanId}", subscription.SubscriptionPlanId);
                    return false;
                }

                var amount = subscription.IsYearly ? plan.PriceYearly : plan.PriceMonthly;
                var nextBillingDate = subscription.EndDate ?? DateTime.UtcNow.AddMonths(subscription.IsYearly ? 12 : 1);

                subscription.StartDate = nextBillingDate;
                subscription.EndDate = nextBillingDate.AddMonths(subscription.IsYearly ? 12 : 1);
                subscription.UpdatedAt = DateTime.UtcNow;

                await CreateInvoiceAsync(userId, amount, "Subscription renewal", subscription.PaymentProvider);
                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Subscription renewed for user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(string userId, decimal amount, string description, PaymentProvider provider)
        {
            try
            {
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var invoice = new Invoice
                {
                    UserId = userId,
                    InvoiceNumber = invoiceNumber,
                    Status = InvoiceStatus.Open,
                    PaymentProvider = provider,
                    Amount = amount,
                    Tax = 0,
                    Total = amount,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice created: {InvoiceNumber} for user {UserId}", invoiceNumber, userId);

                return _mapper.Map<InvoiceDto>(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for user {UserId}", userId);
                throw;
            }
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                    return null;

                return _mapper.Map<InvoiceDto>(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<List<InvoiceDto>> GetUserInvoicesAsync(string userId)
        {
            try
            {
                var invoices = await _invoiceRepository.FindAsync(i => i.UserId == userId);
                var orderedInvoices = invoices.OrderByDescending(i => i.InvoiceDate).ToList();

                return _mapper.Map<List<InvoiceDto>>(orderedInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, string? externalInvoiceId, PaymentProvider provider)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found: {InvoiceId}", invoiceId);
                    return false;
                }

                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
                invoice.UpdatedAt = DateTime.UtcNow;

                if (provider == PaymentProvider.Stripe)
                {
                    invoice.StripeInvoiceId = externalInvoiceId;
                }
                else if (provider == PaymentProvider.PayPal)
                {
                    invoice.PayPalInvoiceId = externalInvoiceId;
                }

                _invoiceRepository.Update(invoice);

                await RecordTransactionAsync(invoice.UserId, TransactionType.Subscription, invoice.Total, 
                    invoice.Description ?? "Invoice payment", provider);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {InvoiceId} marked as paid", invoiceId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
                throw;
            }
        }

        public async Task<bool> MarkInvoiceAsFailedAsync(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found: {InvoiceId}", invoiceId);
                    return false;
                }

                invoice.Status = InvoiceStatus.Uncollectible;
                invoice.UpdatedAt = DateTime.UtcNow;

                _invoiceRepository.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {InvoiceId} marked as failed", invoiceId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as failed", invoiceId);
                throw;
            }
        }

        public async Task<bool> HandlePaymentFailureAsync(string userId, int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null || invoice.UserId != userId)
                {
                    _logger.LogWarning("Invoice not found or user mismatch: {InvoiceId}, {UserId}", invoiceId, userId);
                    return false;
                }

                var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
                    s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

                if (subscription != null)
                {
                    subscription.Status = SubscriptionStatus.PastDue;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    _subscriptionRepository.Update(subscription);
                }

                await MarkInvoiceAsFailedAsync(invoiceId);

                await RecordTransactionAsync(userId, TransactionType.Subscription, invoice.Total,
                    "Payment failed", invoice.PaymentProvider);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogWarning("Payment failure handled for user {UserId}, invoice {InvoiceId}", userId, invoiceId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment failure for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RetryPaymentAsync(string userId, int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null || invoice.UserId != userId)
                {
                    _logger.LogWarning("Invoice not found or user mismatch: {InvoiceId}, {UserId}", invoiceId, userId);
                    return false;
                }

                if (invoice.Status == InvoiceStatus.Paid)
                {
                    _logger.LogWarning("Invoice already paid: {InvoiceId}", invoiceId);
                    return false;
                }

                invoice.Status = InvoiceStatus.Open;
                invoice.UpdatedAt = DateTime.UtcNow;

                _invoiceRepository.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment retry initiated for invoice {InvoiceId}", invoiceId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment for invoice {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<List<InvoiceDto>> GetFailedPaymentsAsync(string userId)
        {
            try
            {
                var failedInvoices = await _invoiceRepository.FindAsync(
                    i => i.UserId == userId && i.Status == InvoiceStatus.Uncollectible);

                var orderedInvoices = failedInvoices.OrderByDescending(i => i.InvoiceDate).ToList();

                return _mapper.Map<List<InvoiceDto>>(orderedInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed payments for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BillingHistoryDto> RecordTransactionAsync(string userId, TransactionType type, decimal amount, string description, PaymentProvider provider)
        {
            try
            {
                var history = new BillingHistory
                {
                    UserId = userId,
                    Type = type,
                    Status = TransactionStatus.Completed,
                    PaymentProvider = provider,
                    Amount = amount,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _billingHistoryRepository.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Transaction recorded for user {UserId}, type {Type}, amount {Amount}", userId, type, amount);

                return _mapper.Map<BillingHistoryDto>(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording transaction for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<BillingHistoryDto>> GetBillingHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = await _billingHistoryRepository.FindAsync(h => h.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(h => h.TransactionDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(h => h.TransactionDate <= endDate.Value);
                }

                var orderedHistory = query.OrderByDescending(h => h.TransactionDate).ToList();

                return _mapper.Map<List<BillingHistoryDto>>(orderedHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                throw;
            }
        }

        private async Task<UserSubscriptionDto> MapToSubscriptionDto(UserSubscription subscription)
        {
            var plan = await _planRepository.GetByIdAsync(subscription.SubscriptionPlanId);

            return new UserSubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                SubscriptionPlanId = subscription.SubscriptionPlanId,
                PlanName = plan?.Name ?? "Unknown",
                Status = subscription.Status,
                PaymentProvider = subscription.PaymentProvider,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                TrialEndDate = subscription.TrialEndDate,
                IsYearly = subscription.IsYearly,
                StripeSubscriptionId = subscription.StripeSubscriptionId,
                PayPalSubscriptionId = subscription.PayPalSubscriptionId,
                CreatedAt = subscription.CreatedAt
            };
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var todayInvoices = await _invoiceRepository.FindAsync(
                i => i.InvoiceNumber.StartsWith($"INV-{datePrefix}"));

            var count = todayInvoices.Count() + 1;
            return $"INV-{datePrefix}-{count:D5}";
        }
    }
}
