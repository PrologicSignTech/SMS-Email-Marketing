using MarketingPlatform.Application.DTOs.Stripe;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace MarketingPlatform.Application.Services
{
    public class StripeService : IStripeService
    {
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IRepository<UserSubscription> _userSubscriptionRepository;
        private readonly IRepository<Core.Entities.Invoice> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;
        private readonly string _webhookSecret;

        public StripeService(
            IRepository<SubscriptionPlan> planRepository,
            IRepository<UserSubscription> userSubscriptionRepository,
            IRepository<Core.Entities.Invoice> invoiceRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<StripeService> logger)
        {
            _planRepository = planRepository;
            _userSubscriptionRepository = userSubscriptionRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;

            var secretKey = _configuration["Stripe:SecretKey"];
            _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;

            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Stripe SecretKey is not configured");
            }
            else
            {
                StripeConfiguration.ApiKey = secretKey;
            }
        }

        public async Task<StripeProductDto> CreateProductAsync(string name, string? description)
        {
            try
            {
                var productService = new ProductService();
                var productOptions = new ProductCreateOptions
                {
                    Name = name,
                    Description = description,
                    Active = true
                };

                var product = await productService.CreateAsync(productOptions);

                _logger.LogInformation("Created Stripe product: {ProductId}", product.Id);

                return new StripeProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Active = product.Active
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe product: {Name}", name);
                throw;
            }
        }

        public async Task<StripeProductDto> UpdateProductAsync(string productId, string name, string? description)
        {
            try
            {
                var productService = new ProductService();
                var productOptions = new ProductUpdateOptions
                {
                    Name = name,
                    Description = description
                };

                var product = await productService.UpdateAsync(productId, productOptions);

                _logger.LogInformation("Updated Stripe product: {ProductId}", productId);

                return new StripeProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Active = product.Active
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error updating Stripe product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(string productId)
        {
            try
            {
                var productService = new ProductService();
                var productOptions = new ProductUpdateOptions
                {
                    Active = false
                };

                await productService.UpdateAsync(productId, productOptions);

                _logger.LogInformation("Archived Stripe product: {ProductId}", productId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error archiving Stripe product: {ProductId}", productId);
                return false;
            }
        }

        public async Task<StripePriceDto> CreatePriceAsync(string productId, decimal amount, string interval)
        {
            try
            {
                var priceService = new PriceService();
                var priceOptions = new PriceCreateOptions
                {
                    Product = productId,
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = interval.ToLowerInvariant()
                    }
                };

                var price = await priceService.CreateAsync(priceOptions);

                _logger.LogInformation("Created Stripe price: {PriceId} for product: {ProductId}", price.Id, productId);

                return new StripePriceDto
                {
                    Id = price.Id,
                    ProductId = productId,
                    Amount = amount,
                    Currency = price.Currency,
                    Interval = interval.ToLowerInvariant()
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe price for product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<StripePriceDto> UpdatePriceAsync(string priceId, bool active)
        {
            try
            {
                var priceService = new PriceService();
                var priceOptions = new PriceUpdateOptions
                {
                    Active = active
                };

                var price = await priceService.UpdateAsync(priceId, priceOptions);

                _logger.LogInformation("Updated Stripe price: {PriceId} active status to {Active}", priceId, active);

                return new StripePriceDto
                {
                    Id = price.Id,
                    ProductId = price.ProductId,
                    Amount = (decimal)(price.UnitAmount ?? 0) / 100,
                    Currency = price.Currency,
                    Interval = price.Recurring?.Interval ?? "month"
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error updating Stripe price: {PriceId}", priceId);
                throw;
            }
        }

        public async Task<List<StripePriceDto>> GetProductPricesAsync(string productId)
        {
            try
            {
                var priceService = new PriceService();
                var priceOptions = new PriceListOptions
                {
                    Product = productId
                };

                var prices = await priceService.ListAsync(priceOptions);

                return prices.Select(p => new StripePriceDto
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    Amount = (decimal)(p.UnitAmount ?? 0) / 100,
                    Currency = p.Currency,
                    Interval = p.Recurring?.Interval ?? "month"
                }).ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error getting prices for Stripe product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> SyncPlanToStripeAsync(int planId)
        {
            try
            {
                var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);
                if (plan == null)
                {
                    _logger.LogWarning("Plan not found: {PlanId}", planId);
                    return false;
                }

                // Create or update Stripe product
                string productId;
                if (string.IsNullOrEmpty(plan.StripeProductId))
                {
                    var product = await CreateProductAsync(plan.Name, plan.Description);
                    productId = product.Id;
                    plan.StripeProductId = productId;
                }
                else
                {
                    var product = await UpdateProductAsync(plan.StripeProductId, plan.Name, plan.Description);
                    productId = product.Id;
                }

                // Create or update monthly price
                if (plan.PriceMonthly > 0)
                {
                    if (string.IsNullOrEmpty(plan.StripePriceIdMonthly))
                    {
                        var monthlyPrice = await CreatePriceAsync(productId, plan.PriceMonthly, "month");
                        plan.StripePriceIdMonthly = monthlyPrice.Id;
                    }
                }

                // Create or update yearly price
                if (plan.PriceYearly > 0)
                {
                    if (string.IsNullOrEmpty(plan.StripePriceIdYearly))
                    {
                        var yearlyPrice = await CreatePriceAsync(productId, plan.PriceYearly, "year");
                        plan.StripePriceIdYearly = yearlyPrice.Id;
                    }
                }

                _planRepository.Update(plan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully synced plan {PlanId} to Stripe", planId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing plan {PlanId} to Stripe", planId);
                return false;
            }
        }

        public async Task<bool> SyncAllPlansToStripeAsync()
        {
            try
            {
                var plans = await _planRepository.FindAsync(p => p.IsActive && !p.IsDeleted);
                var allSuccess = true;

                foreach (var plan in plans)
                {
                    var success = await SyncPlanToStripeAsync(plan.Id);
                    if (!success)
                        allSuccess = false;
                }

                _logger.LogInformation("Completed syncing all plans to Stripe. Success: {Success}", allSuccess);
                return allSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing all plans to Stripe");
                return false;
            }
        }

        public async Task<bool> HandleWebhookEventAsync(string json, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookSecret))
                {
                    _logger.LogWarning("Stripe webhook secret is not configured");
                    return false;
                }

                var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

                _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "invoice.payment_succeeded":
                        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                        if (invoice != null)
                            await ProcessInvoicePaidAsync(invoice.Id);
                        break;

                    case "invoice.payment_failed":
                        var failedInvoice = stripeEvent.Data.Object as Stripe.Invoice;
                        if (failedInvoice != null)
                            await ProcessInvoicePaymentFailedAsync(failedInvoice.Id);
                        break;

                    case "customer.subscription.updated":
                        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                        if (subscription != null)
                            await ProcessSubscriptionUpdatedAsync(subscription.Id);
                        break;

                    case "customer.subscription.deleted":
                        var deletedSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                        if (deletedSubscription != null)
                            await ProcessSubscriptionDeletedAsync(deletedSubscription.Id);
                        break;

                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook event");
                return false;
            }
        }

        public async Task ProcessInvoicePaidAsync(string invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.StripeInvoiceId == invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found for Stripe invoice: {InvoiceId}", invoiceId);
                    return;
                }

                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
                invoice.UpdatedAt = DateTime.UtcNow;

                _invoiceRepository.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked invoice {InvoiceId} as paid", invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing paid invoice: {InvoiceId}", invoiceId);
            }
        }

        public async Task ProcessInvoicePaymentFailedAsync(string invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.StripeInvoiceId == invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found for Stripe invoice: {InvoiceId}", invoiceId);
                    return;
                }

                invoice.Status = InvoiceStatus.Uncollectible;
                invoice.UpdatedAt = DateTime.UtcNow;

                _invoiceRepository.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked invoice {InvoiceId} as failed", invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed invoice: {InvoiceId}", invoiceId);
            }
        }

        public async Task ProcessSubscriptionUpdatedAsync(string subscriptionId)
        {
            try
            {
                var userSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(
                    s => s.StripeSubscriptionId == subscriptionId);

                if (userSubscription == null)
                {
                    _logger.LogWarning("User subscription not found for Stripe subscription: {SubscriptionId}", subscriptionId);
                    return;
                }

                // Get subscription from Stripe to check status
                var subscriptionService = new SubscriptionService();
                var stripeSubscription = await subscriptionService.GetAsync(subscriptionId);

                // Update status based on Stripe subscription status
                userSubscription.Status = stripeSubscription.Status switch
                {
                    "active" => SubscriptionStatus.Active,
                    "trialing" => SubscriptionStatus.Trial,
                    "past_due" => SubscriptionStatus.PastDue,
                    "canceled" => SubscriptionStatus.Canceled,
                    "unpaid" => SubscriptionStatus.PastDue,
                    _ => userSubscription.Status
                };

                userSubscription.UpdatedAt = DateTime.UtcNow;

                _userSubscriptionRepository.Update(userSubscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated subscription {SubscriptionId} status to {Status}", 
                    userSubscription.Id, userSubscription.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription update: {SubscriptionId}", subscriptionId);
            }
        }

        public async Task ProcessSubscriptionDeletedAsync(string subscriptionId)
        {
            try
            {
                var userSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(
                    s => s.StripeSubscriptionId == subscriptionId);

                if (userSubscription == null)
                {
                    _logger.LogWarning("User subscription not found for Stripe subscription: {SubscriptionId}", subscriptionId);
                    return;
                }

                userSubscription.Status = SubscriptionStatus.Canceled;
                userSubscription.CanceledAt = DateTime.UtcNow;
                userSubscription.EndDate = DateTime.UtcNow;
                userSubscription.UpdatedAt = DateTime.UtcNow;

                _userSubscriptionRepository.Update(userSubscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Canceled subscription {SubscriptionId}", userSubscription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription deletion: {SubscriptionId}", subscriptionId);
            }
        }
    }
}
