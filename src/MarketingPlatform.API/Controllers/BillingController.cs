using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Billing;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/billing")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            IBillingService billingService,
            ILogger<BillingController> logger)
        {
            _billingService = billingService;
            _logger = logger;
        }

        [HttpGet("subscription")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> GetSubscription()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var subscription = await _billingService.GetUserSubscriptionAsync(userId);
                if (subscription == null)
                    return NotFound(ApiResponse<UserSubscriptionDto>.ErrorResponse("No active subscription found"));

                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription for user {UserId}", userId);
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse("Failed to retrieve subscription", new List<string> { ex.Message }));
            }
        }

        [HttpPost("subscribe")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> Subscribe([FromBody] CreateSubscriptionDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var subscription = await _billingService.CreateSubscriptionAsync(dto);
                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription, "Subscription created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for user {UserId}", userId);
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse("Failed to create subscription", new List<string> { ex.Message }));
            }
        }

        [HttpPost("upgrade")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> Upgrade([FromBody] SubscriptionUpgradeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var subscription = await _billingService.UpgradeSubscriptionAsync(userId, dto);
                if (subscription == null)
                    return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse("Failed to upgrade subscription"));

                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription, "Subscription upgraded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading subscription for user {UserId}", userId);
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse("Failed to upgrade subscription", new List<string> { ex.Message }));
            }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> Cancel([FromBody] string reason)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _billingService.CancelSubscriptionAsync(userId, reason);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel subscription"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Subscription cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription for user {UserId}", userId);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel subscription", new List<string> { ex.Message }));
            }
        }

        [HttpGet("invoices")]
        public async Task<ActionResult<ApiResponse<List<InvoiceDto>>>> GetInvoices()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var invoices = await _billingService.GetUserInvoicesAsync(userId);
                return Ok(ApiResponse<List<InvoiceDto>>.SuccessResponse(invoices));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices for user {UserId}", userId);
                return BadRequest(ApiResponse<List<InvoiceDto>>.ErrorResponse("Failed to retrieve invoices", new List<string> { ex.Message }));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<BillingHistoryDto>>>> GetHistory(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var history = await _billingService.GetBillingHistoryAsync(userId, startDate, endDate);
                return Ok(ApiResponse<List<BillingHistoryDto>>.SuccessResponse(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                return BadRequest(ApiResponse<List<BillingHistoryDto>>.ErrorResponse("Failed to retrieve billing history", new List<string> { ex.Message }));
            }
        }
    }
}
