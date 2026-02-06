using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/webhooks/paypal")]
    public class PayPalWebhooksController : ControllerBase
    {
        private readonly IPayPalService _payPalService;
        private readonly ILogger<PayPalWebhooksController> _logger;

        public PayPalWebhooksController(
            IPayPalService payPalService,
            ILogger<PayPalWebhooksController> logger)
        {
            _payPalService = payPalService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();

                var signature = Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString();
                
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing PayPal signature headers");
                    return BadRequest("Missing PayPal signature headers");
                }

                var result = await _payPalService.HandleWebhookEventAsync(json, signature);
                
                if (result)
                {
                    return Ok();
                }
                
                _logger.LogWarning("Failed to process PayPal webhook");
                return BadRequest("Failed to process webhook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return BadRequest(ex.Message);
            }
        }
    }
}
