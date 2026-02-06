using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/webhooks/stripe")]
    public class StripeWebhooksController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeWebhooksController> _logger;

        public StripeWebhooksController(
            IStripeService stripeService,
            ILogger<StripeWebhooksController> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();

                var signature = Request.Headers["Stripe-Signature"].ToString();
                
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing Stripe-Signature header");
                    return BadRequest("Missing Stripe-Signature header");
                }

                var result = await _stripeService.HandleWebhookEventAsync(json, signature);
                
                if (result)
                {
                    return Ok();
                }
                
                _logger.LogWarning("Failed to process Stripe webhook");
                return BadRequest("Failed to process webhook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return BadRequest(ex.Message);
            }
        }
    }
}
