using MarketingPlatform.Application.DTOs.PayPal;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using PayPalHttp;
using System.Text;
using Json = System.Text.Json.JsonSerializer;
using JsonElement = System.Text.Json.JsonElement;
using JsonValueKind = System.Text.Json.JsonValueKind;

namespace MarketingPlatform.Application.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IRepository<UserSubscription> _userSubscriptionRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalService> _logger;
        private readonly PayPalHttpClient _client;
        private readonly string _webhookId;

        public PayPalService(
            IRepository<SubscriptionPlan> planRepository,
            IRepository<UserSubscription> userSubscriptionRepository,
            IRepository<Invoice> invoiceRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<PayPalService> logger)
        {
            _planRepository = planRepository;
            _userSubscriptionRepository = userSubscriptionRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;

            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];
            var mode = _configuration["PayPal:Mode"] ?? "sandbox";
            _webhookId = _configuration["PayPal:WebhookId"] ?? string.Empty;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("PayPal credentials are not configured");
                _client = null!;
            }
            else
            {
                var environment = mode.ToLower() == "live"
                    ? new LiveEnvironment(clientId, clientSecret)
                    : (PayPalEnvironment)new SandboxEnvironment(clientId, clientSecret);

                _client = new PayPalHttpClient(environment);
            }
        }

        public async Task<PayPalProductDto> CreateProductAsync(string name, string? description)
        {
            try
            {
                if (_client == null)
                {
                    _logger.LogError("PayPal client is not configured");
                    throw new InvalidOperationException("PayPal credentials are not configured");
                }

                var request = new HttpRequest("/v1/catalogs/products", HttpMethod.Post)
                {
                    ContentType = "application/json"
                };

                var productData = new
                {
                    name,
                    description = description ?? string.Empty,
                    type = "SERVICE",
                    category = "SOFTWARE"
                };

                request.Body = productData;
                var response = await _client.Execute(request);
                var result = Json.Deserialize<Dictionary<string, JsonElement>>(
                    Json.Serialize(response.Result<object>()));

                var productId = result?["id"].GetString() ?? string.Empty;

                _logger.LogInformation("Created PayPal product: {ProductId}", productId);

                return new PayPalProductDto
                {
                    Id = productId,
                    Name = name,
                    Description = description,
                    Type = "SERVICE",
                    Category = "SOFTWARE"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal product: {Name}", name);
                throw;
            }
        }

        public async Task<PayPalProductDto> UpdateProductAsync(string productId, string name, string? description)
        {
            try
            {
                if (_client == null)
                {
                    _logger.LogError("PayPal client is not configured");
                    throw new InvalidOperationException("PayPal credentials are not configured");
                }

                var request = new HttpRequest($"/v1/catalogs/products/{productId}", new HttpMethod("PATCH"))
                {
                    ContentType = "application/json"
                };

                var patches = new[]
                {
                    new { op = "replace", path = "/name", value = name },
                    new { op = "replace", path = "/description", value = description ?? string.Empty }
                };

                request.Body = patches;
                await _client.Execute(request);

                _logger.LogInformation("Updated PayPal product: {ProductId}", productId);

                return new PayPalProductDto
                {
                    Id = productId,
                    Name = name,
                    Description = description,
                    Type = "SERVICE",
                    Category = "SOFTWARE"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PayPal product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(string productId)
        {
            try
            {
                // PayPal doesn't allow deletion, so we deactivate the product
                _logger.LogInformation("PayPal products cannot be deleted, only deactivated: {ProductId}", productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating PayPal product: {ProductId}", productId);
                return false;
            }
        }

        public async Task<PayPalPlanDto> CreatePlanAsync(string productId, string name, decimal amount, string billingCycle)
        {
            try
            {
                if (_client == null)
                {
                    _logger.LogError("PayPal client is not configured");
                    throw new InvalidOperationException("PayPal credentials are not configured");
                }

                var request = new HttpRequest("/v1/billing/plans", HttpMethod.Post)
                {
                    ContentType = "application/json"
                };

                var interval = billingCycle.ToUpperInvariant() == "YEAR" ? "YEAR" : "MONTH";
                var planData = new
                {
                    product_id = productId,
                    name,
                    status = "ACTIVE",
                    billing_cycles = new[]
                    {
                        new
                        {
                            frequency = new
                            {
                                interval_unit = interval,
                                interval_count = 1
                            },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = 0,
                            pricing_scheme = new
                            {
                                fixed_price = new
                                {
                                    value = amount.ToString("F2"),
                                    currency_code = "USD"
                                }
                            }
                        }
                    },
                    payment_preferences = new
                    {
                        auto_bill_outstanding = true,
                        setup_fee_failure_action = "CONTINUE",
                        payment_failure_threshold = 3
                    }
                };

                request.Body = planData;
                var response = await _client.Execute(request);
                var result = Json.Deserialize<Dictionary<string, JsonElement>>(
                    Json.Serialize(response.Result<object>()));

                var planId = result?["id"].GetString() ?? string.Empty;

                _logger.LogInformation("Created PayPal plan: {PlanId} for product: {ProductId}", planId, productId);

                return new PayPalPlanDto
                {
                    Id = planId,
                    ProductId = productId,
                    Name = name,
                    Status = "ACTIVE",
                    BillingCycle = interval,
                    Amount = amount,
                    Currency = "USD"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal plan for product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<PayPalPlanDto> UpdatePlanAsync(string planId, bool active)
        {
            try
            {
                if (_client == null)
                {
                    _logger.LogError("PayPal client is not configured");
                    throw new InvalidOperationException("PayPal credentials are not configured");
                }

                var request = new HttpRequest($"/v1/billing/plans/{planId}", new HttpMethod("PATCH"))
                {
                    ContentType = "application/json"
                };

                var patches = new[]
                {
                    new { op = "replace", path = "/status", value = active ? "ACTIVE" : "INACTIVE" }
                };

                request.Body = patches;
                await _client.Execute(request);

                _logger.LogInformation("Updated PayPal plan: {PlanId} active status to {Active}", planId, active);

                return new PayPalPlanDto
                {
                    Id = planId,
                    Status = active ? "ACTIVE" : "INACTIVE"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PayPal plan: {PlanId}", planId);
                throw;
            }
        }

        public async Task<List<PayPalPlanDto>> GetProductPlansAsync(string productId)
        {
            try
            {
                if (_client == null)
                {
                    _logger.LogError("PayPal client is not configured");
                    throw new InvalidOperationException("PayPal credentials are not configured");
                }

                var request = new HttpRequest($"/v1/billing/plans?product_id={productId}", HttpMethod.Get);
                var response = await _client.Execute(request);
                var result = Json.Deserialize<Dictionary<string, JsonElement>>(
                    Json.Serialize(response.Result<object>()));

                var plans = new List<PayPalPlanDto>();

                if (result?.ContainsKey("plans") == true && result["plans"].ValueKind == JsonValueKind.Array)
                {
                    foreach (var planElement in result["plans"].EnumerateArray())
                    {
                        plans.Add(new PayPalPlanDto
                        {
                            Id = planElement.GetProperty("id").GetString() ?? string.Empty,
                            ProductId = productId,
                            Name = planElement.GetProperty("name").GetString() ?? string.Empty,
                            Status = planElement.GetProperty("status").GetString() ?? "ACTIVE"
                        });
                    }
                }

                return plans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plans for PayPal product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> SyncPlanToPayPalAsync(int planId)
        {
            try
            {
                var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);
                if (plan == null)
                {
                    _logger.LogWarning("Plan not found: {PlanId}", planId);
                    return false;
                }

                // Create or update PayPal product
                string productId;
                if (string.IsNullOrEmpty(plan.PayPalProductId))
                {
                    var product = await CreateProductAsync(plan.Name, plan.Description);
                    productId = product.Id;
                    plan.PayPalProductId = productId;
                }
                else
                {
                    var product = await UpdateProductAsync(plan.PayPalProductId, plan.Name, plan.Description);
                    productId = product.Id;
                }

                // Create monthly plan
                if (plan.PriceMonthly > 0)
                {
                    if (string.IsNullOrEmpty(plan.PayPalPlanIdMonthly))
                    {
                        var monthlyPlan = await CreatePlanAsync(productId, $"{plan.Name} - Monthly", plan.PriceMonthly, "MONTH");
                        plan.PayPalPlanIdMonthly = monthlyPlan.Id;
                    }
                }

                // Create yearly plan
                if (plan.PriceYearly > 0)
                {
                    if (string.IsNullOrEmpty(plan.PayPalPlanIdYearly))
                    {
                        var yearlyPlan = await CreatePlanAsync(productId, $"{plan.Name} - Yearly", plan.PriceYearly, "YEAR");
                        plan.PayPalPlanIdYearly = yearlyPlan.Id;
                    }
                }

                _planRepository.Update(plan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully synced plan {PlanId} to PayPal", planId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing plan {PlanId} to PayPal", planId);
                return false;
            }
        }

        public async Task<bool> SyncAllPlansToPayPalAsync()
        {
            try
            {
                var plans = await _planRepository.FindAsync(p => p.IsActive && !p.IsDeleted);
                var allSuccess = true;

                foreach (var plan in plans)
                {
                    var success = await SyncPlanToPayPalAsync(plan.Id);
                    if (!success)
                        allSuccess = false;
                }

                _logger.LogInformation("Completed syncing all plans to PayPal. Success: {Success}", allSuccess);
                return allSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing all plans to PayPal");
                return false;
            }
        }

        public async Task<bool> HandleWebhookEventAsync(string json, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookId))
                {
                    _logger.LogWarning("PayPal webhook ID is not configured");
                    return false;
                }

                var webhookEvent = Json.Deserialize<Dictionary<string, JsonElement>>(json);
                if (webhookEvent == null)
                {
                    _logger.LogWarning("Invalid PayPal webhook event JSON");
                    return false;
                }

                var eventType = webhookEvent.ContainsKey("event_type") 
                    ? webhookEvent["event_type"].GetString() 
                    : string.Empty;

                _logger.LogInformation("Processing PayPal webhook event: {EventType}", eventType);

                switch (eventType)
                {
                    case "BILLING.SUBSCRIPTION.ACTIVATED":
                        if (webhookEvent.ContainsKey("resource"))
                        {
                            var resource = webhookEvent["resource"];
                            var activatedSubId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                            if (!string.IsNullOrEmpty(activatedSubId))
                                await ProcessBillingSubscriptionActivatedAsync(activatedSubId);
                        }
                        break;

                    case "BILLING.SUBSCRIPTION.CANCELLED":
                        if (webhookEvent.ContainsKey("resource"))
                        {
                            var resource = webhookEvent["resource"];
                            var cancelledSubId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                            if (!string.IsNullOrEmpty(cancelledSubId))
                                await ProcessBillingSubscriptionCancelledAsync(cancelledSubId);
                        }
                        break;

                    case "PAYMENT.SALE.COMPLETED":
                        if (webhookEvent.ContainsKey("resource"))
                        {
                            var resource = webhookEvent["resource"];
                            var saleId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                            if (!string.IsNullOrEmpty(saleId))
                                await ProcessPaymentSaleCompletedAsync(saleId);
                        }
                        break;

                    case "PAYMENT.SALE.REFUNDED":
                        if (webhookEvent.ContainsKey("resource"))
                        {
                            var resource = webhookEvent["resource"];
                            var refundedSaleId = resource.TryGetProperty("parent_payment", out var idProp) ? idProp.GetString() : null;
                            if (!string.IsNullOrEmpty(refundedSaleId))
                                await ProcessPaymentSaleRefundedAsync(refundedSaleId);
                        }
                        break;

                    default:
                        _logger.LogInformation("Unhandled PayPal event type: {EventType}", eventType);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook event");
                return false;
            }
        }

        public async Task ProcessBillingSubscriptionActivatedAsync(string subscriptionId)
        {
            try
            {
                var userSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(
                    s => s.PayPalSubscriptionId == subscriptionId);

                if (userSubscription == null)
                {
                    _logger.LogWarning("User subscription not found for PayPal subscription: {SubscriptionId}", subscriptionId);
                    return;
                }

                userSubscription.Status = SubscriptionStatus.Active;
                userSubscription.UpdatedAt = DateTime.UtcNow;

                _userSubscriptionRepository.Update(userSubscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Activated subscription {SubscriptionId}", userSubscription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription activation: {SubscriptionId}", subscriptionId);
            }
        }

        public async Task ProcessBillingSubscriptionCancelledAsync(string subscriptionId)
        {
            try
            {
                var userSubscription = await _userSubscriptionRepository.FirstOrDefaultAsync(
                    s => s.PayPalSubscriptionId == subscriptionId);

                if (userSubscription == null)
                {
                    _logger.LogWarning("User subscription not found for PayPal subscription: {SubscriptionId}", subscriptionId);
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
                _logger.LogError(ex, "Error processing subscription cancellation: {SubscriptionId}", subscriptionId);
            }
        }

        public async Task ProcessPaymentSaleCompletedAsync(string saleId)
        {
            try
            {
                var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.PayPalInvoiceId == saleId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found for PayPal sale: {SaleId}", saleId);
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
                _logger.LogError(ex, "Error processing payment sale completed: {SaleId}", saleId);
            }
        }

        public async Task ProcessPaymentSaleRefundedAsync(string saleId)
        {
            try
            {
                var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.PayPalInvoiceId == saleId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found for PayPal sale: {SaleId}", saleId);
                    return;
                }

                invoice.Status = InvoiceStatus.Void;
                invoice.UpdatedAt = DateTime.UtcNow;

                _invoiceRepository.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked invoice {InvoiceId} as refunded", invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment sale refunded: {SaleId}", saleId);
            }
        }
    }
}
