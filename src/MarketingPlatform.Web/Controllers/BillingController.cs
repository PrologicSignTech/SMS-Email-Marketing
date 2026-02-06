/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<BillingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public BillingController(IApiClient apiClient, ILogger<BillingController> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _apiClient.SetAuthorizationToken(token);
            }
        }

        public IActionResult Index()
        {
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View();
        }

        public IActionResult Subscribe()
        {
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View();
        }

        public IActionResult PaymentHistory()
        {
            return View();
        }

        public IActionResult Invoices()
        {
            return View();
        }

        public IActionResult Usage()
        {
            return View();
        }

        /// <summary>
        /// Get billing subscription details via server-side API call
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubscription()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/billing/subscription");
                if (result?.Success == true)
                {
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = false, message = result?.Message ?? "Failed to load subscription data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscription data");
                return Json(new { success = false, message = "An error occurred while loading subscription data" });
            }
        }

        /// <summary>
        /// Get payment history via server-side API call
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaymentHistory()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/billing/payments");
                if (result?.Success == true)
                {
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = false, message = result?.Message ?? "Failed to load payment history" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment history");
                return Json(new { success = false, message = "An error occurred while loading payment history" });
            }
        }

        /// <summary>
        /// Get invoices via server-side API call
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/billing/invoices");
                if (result?.Success == true)
                {
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = false, message = result?.Message ?? "Failed to load invoices" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoices");
                return Json(new { success = false, message = "An error occurred while loading invoices" });
            }
        }

        /// <summary>
        /// Get usage data via server-side API call
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsage()
        {
            try
            {
                var result = await _apiClient.GetAsync<ApiResponse<object>>("/api/billing/usage");
                if (result?.Success == true)
                {
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = false, message = result?.Message ?? "Failed to load usage data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading usage data");
                return Json(new { success = false, message = "An error occurred while loading usage data" });
            }
        }
    }
}
