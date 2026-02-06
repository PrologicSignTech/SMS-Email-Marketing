using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MarketingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;
        private readonly IConfiguration _configuration;

        public WebhooksController(
            IWebhookService webhookService,
            ILogger<WebhooksController> logger,
            IConfiguration configuration)
        {
            _webhookService = webhookService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("message-status")]
        public async Task<IActionResult> MessageStatusUpdate(
            [FromBody] MessageStatusUpdateDto dto,
            [FromHeader(Name = "X-Webhook-Signature")] string? signature = null)
        {
            try
            {
                // Validate webhook signature if provided
                if (!string.IsNullOrEmpty(signature))
                {
                    var webhookSecret = _configuration["WebhookSettings:Secret"];
                    if (string.IsNullOrEmpty(webhookSecret))
                    {
                        _logger.LogError("Webhook secret not configured");
                        return StatusCode(500, new { success = false, error = "Webhook secret not configured" });
                    }
                    
                    var payload = System.Text.Json.JsonSerializer.Serialize(dto);
                    
                    if (!_webhookService.ValidateWebhookSignature(signature, payload, webhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature received");
                        return Unauthorized(new { success = false, error = "Invalid signature" });
                    }
                }

                _logger.LogInformation("Webhook received for message {ExternalId}: {Status}", 
                    dto.ExternalMessageId, dto.Status);

                var result = await _webhookService.ProcessMessageStatusUpdateAsync(
                    dto.ExternalMessageId, 
                    dto.Status.ToString(), 
                    dto.ErrorMessage);

                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return NotFound(new { success = false, error = "Message not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("sms-inbound")]
        public async Task<IActionResult> SMSInbound(
            [FromBody] InboundSmsWebhookDto payload,
            [FromHeader(Name = "X-Webhook-Signature")] string? signature = null)
        {
            try
            {
                // Validate webhook signature if provided
                if (!string.IsNullOrEmpty(signature))
                {
                    var webhookSecret = _configuration["WebhookSettings:Secret"];
                    if (string.IsNullOrEmpty(webhookSecret))
                    {
                        _logger.LogError("Webhook secret not configured");
                        return StatusCode(500, new { success = false, error = "Webhook secret not configured" });
                    }
                    
                    var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
                    
                    if (!_webhookService.ValidateWebhookSignature(signature, payloadJson, webhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature for inbound SMS");
                        return Unauthorized(new { success = false, error = "Invalid signature" });
                    }
                }

                _logger.LogInformation("Inbound SMS received from {PhoneNumber}: {Message}", 
                    payload.From, payload.Body);

                // Check for STOP/UNSUBSCRIBE keywords
                if (IsOptOutKeyword(payload.Body))
                {
                    await _webhookService.ProcessOptOutAsync(payload.From, "SMS_INBOUND");
                    return Ok(new { success = true, message = "Opt-out processed" });
                }

                // Process inbound message (including keywords)
                var result = await _webhookService.ProcessInboundMessageAsync(
                    payload.From, 
                    payload.To, 
                    payload.Body, 
                    payload.MessageSid);

                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process inbound SMS");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("sms-delivery")]
        public async Task<IActionResult> SMSDeliveryUpdate(
            [FromBody] DeliveryStatusDto payload,
            [FromQuery] string externalMessageId,
            [FromHeader(Name = "X-Webhook-Signature")] string? signature = null)
        {
            try
            {
                // Validate webhook signature if provided
                if (!string.IsNullOrEmpty(signature))
                {
                    var webhookSecret = _configuration["WebhookSettings:Secret"];
                    if (string.IsNullOrEmpty(webhookSecret))
                    {
                        _logger.LogError("Webhook secret not configured");
                        return StatusCode(500, new { success = false, error = "Webhook secret not configured" });
                    }
                    
                    var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
                    
                    if (!_webhookService.ValidateWebhookSignature(signature, payloadJson, webhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature for SMS delivery");
                        return Unauthorized(new { success = false, error = "Invalid signature" });
                    }
                }

                _logger.LogInformation("SMS delivery webhook received for {ExternalId}: {Status}", 
                    externalMessageId, payload.Status);

                var result = await _webhookService.ProcessDeliveryStatusAsync(externalMessageId, payload);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SMS delivery webhook");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("email-delivery")]
        public async Task<IActionResult> EmailDeliveryUpdate(
            [FromBody] DeliveryStatusDto payload,
            [FromQuery] string externalMessageId,
            [FromHeader(Name = "X-Webhook-Signature")] string? signature = null)
        {
            try
            {
                // Validate webhook signature if provided
                if (!string.IsNullOrEmpty(signature))
                {
                    var webhookSecret = _configuration["WebhookSettings:Secret"];
                    if (string.IsNullOrEmpty(webhookSecret))
                    {
                        _logger.LogError("Webhook secret not configured");
                        return StatusCode(500, new { success = false, error = "Webhook secret not configured" });
                    }
                    
                    var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
                    
                    if (!_webhookService.ValidateWebhookSignature(signature, payloadJson, webhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature for email delivery");
                        return Unauthorized(new { success = false, error = "Invalid signature" });
                    }
                }

                _logger.LogInformation("Email delivery webhook received for {ExternalId}: {Status}", 
                    externalMessageId, payload.Status);

                var result = await _webhookService.ProcessDeliveryStatusAsync(externalMessageId, payload);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email delivery webhook");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("opt-out")]
        public async Task<IActionResult> ProcessOptOut([FromBody] OptOutWebhookDto payload)
        {
            try
            {
                _logger.LogInformation("Opt-out webhook received for {PhoneNumber}", payload.PhoneNumber);

                var result = await _webhookService.ProcessOptOutAsync(payload.PhoneNumber, payload.Source);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Opt-out processed successfully" });
                }
                else
                {
                    return NotFound(new { success = false, error = "Contact not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing opt-out webhook");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        private bool IsOptOutKeyword(string message)
        {
            var optOutKeywords = new[] { "STOP", "UNSUBSCRIBE", "CANCEL", "END", "QUIT", "OPTOUT" };
            return optOutKeywords.Any(keyword => 
                message.Trim().Equals(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }

    // DTO for inbound SMS webhook (Twilio format)
    public class InboundSmsWebhookDto
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string MessageSid { get; set; } = string.Empty;
    }

    public class OptOutWebhookDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Source { get; set; }
    }
}
