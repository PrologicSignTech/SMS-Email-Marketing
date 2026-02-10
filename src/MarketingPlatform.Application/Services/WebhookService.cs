using System.Security.Cryptography;
using System.Text;
using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<SuppressionList> _suppressionRepository;
        private readonly IRepository<PhoneNumber> _phoneNumberRepository;
        private readonly IKeywordService _keywordService;
        private readonly IEventTriggerService _eventTriggerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(
            IRepository<CampaignMessage> messageRepository,
            IRepository<Contact> contactRepository,
            IRepository<SuppressionList> suppressionRepository,
            IRepository<PhoneNumber> phoneNumberRepository,
            IKeywordService keywordService,
            IEventTriggerService eventTriggerService,
            IUnitOfWork unitOfWork,
            ILogger<WebhookService> logger)
        {
            _messageRepository = messageRepository;
            _contactRepository = contactRepository;
            _suppressionRepository = suppressionRepository;
            _phoneNumberRepository = phoneNumberRepository;
            _keywordService = keywordService;
            _eventTriggerService = eventTriggerService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> ProcessMessageStatusUpdateAsync(string externalMessageId, string status, string? errorMessage = null)
        {
            try
            {
                var message = await _messageRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(m => m.ExternalMessageId == externalMessageId);

                if (message == null)
                {
                    _logger.LogWarning("Message not found for external ID: {ExternalMessageId}", externalMessageId);
                    return false;
                }

                // Update message status based on provider status
                message.Status = MapProviderStatusToMessageStatus(status);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message.ErrorMessage = errorMessage;
                }

                if (message.Status == MessageStatus.Delivered)
                {
                    message.DeliveredAt = DateTime.UtcNow;
                }
                else if (message.Status == MessageStatus.Failed)
                {
                    message.FailedAt = DateTime.UtcNow;
                }

                _messageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated message {MessageId} status to {Status}", message.Id, message.Status);

                // Trigger workflow events based on delivery status
                if (message.ContactId > 0)
                {
                    var eventData = new Dictionary<string, object>
                    {
                        { "messageId", message.Id },
                        { "externalMessageId", externalMessageId },
                        { "status", status }
                    };

                    if (message.Status == MessageStatus.Delivered)
                    {
                        await _eventTriggerService.TriggerEventAsync(EventType.MessageDelivered, message.ContactId, eventData);
                    }
                    else if (message.Status == MessageStatus.Failed)
                    {
                        eventData["errorMessage"] = errorMessage ?? "";
                        await _eventTriggerService.TriggerEventAsync(EventType.MessageFailed, message.ContactId, eventData);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message status update for {ExternalMessageId}", externalMessageId);
                return false;
            }
        }

        public async Task<bool> ProcessInboundMessageAsync(string from, string to, string body, string? externalId = null)
        {
            try
            {
                _logger.LogInformation("Processing inbound message from {From} to {To}: {Body}", from, to, body);

                // ── Step 1: Find which user owns the "to" number ──
                // The "to" number is the platform number assigned to a specific user.
                // This is the primary isolation mechanism — only that user's workflows trigger.
                var assignedPhone = await _phoneNumberRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(p => p.Number == to && p.AssignedToUserId != null && !p.IsDeleted);

                if (assignedPhone == null)
                {
                    _logger.LogWarning("Inbound to number {To} is not assigned to any user — skipping", to);
                    return true;
                }

                var userId = assignedPhone.AssignedToUserId!;
                _logger.LogInformation("Inbound number {To} belongs to user {UserId}", to, userId);

                // ── Step 2: Process keywords for this user ──
                await _keywordService.ProcessInboundKeywordAsync(from, body);

                // ── Step 3: Find the contact with this "from" phone that belongs to the SAME user ──
                var contact = await _contactRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.PhoneNumber == from && c.UserId == userId && !c.IsDeleted);

                if (contact == null)
                {
                    _logger.LogInformation("No contact found for phone {From} under user {UserId} — skipping workflow triggers", from, userId);
                    return true;
                }

                // ── Step 4: Trigger workflows for this specific user's contact ──
                var keyword = body.Trim().Split(' ')[0].ToUpperInvariant();

                var eventData = new Dictionary<string, object>
                {
                    { "keyword", keyword },
                    { "fullMessage", body },
                    { "from", from },
                    { "to", to }
                };

                // TriggerEventAsync + ProcessKeywordTriggerAsync both verify contact.UserId internally
                await _eventTriggerService.TriggerEventAsync(EventType.KeywordReceived, contact.Id, eventData);
                await _eventTriggerService.ProcessKeywordTriggerAsync(keyword, contact.Id);

                _logger.LogInformation("Triggered workflow events for contact {ContactId} (user {UserId}) via number {To}",
                    contact.Id, userId, to);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inbound message from {From} to {To}", from, to);
                return false;
            }
        }

        public async Task<bool> ProcessOptOutAsync(string phoneNumber, string? source = null)
        {
            try
            {
                // Find contact by phone number
                var contact = await _contactRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

                if (contact == null)
                {
                    _logger.LogWarning("Contact not found for opt-out: {PhoneNumber}", phoneNumber);
                    return false;
                }

                // Check if already in suppression list
                var existingSuppression = await _suppressionRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.PhoneOrEmail == phoneNumber && s.UserId == contact.UserId);

                if (existingSuppression != null)
                {
                    _logger.LogInformation("Contact {PhoneNumber} already in suppression list", phoneNumber);
                    return true;
                }

                // Create suppression record
                var suppression = new SuppressionList
                {
                    PhoneOrEmail = phoneNumber,
                    UserId = contact.UserId,
                    Type = SuppressionType.OptOut,
                    Reason = $"Opt-out via {source ?? "Webhook"}",
                    CreatedAt = DateTime.UtcNow
                };

                await _suppressionRepository.AddAsync(suppression);

                // Update contact opt-in status
                contact.SmsOptIn = false;
                contact.MmsOptIn = false;
                _contactRepository.Update(contact);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Processed opt-out for {PhoneNumber} via {Source}", phoneNumber, source);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing opt-out for {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> ProcessDeliveryStatusAsync(string externalMessageId, DeliveryStatusDto statusDto)
        {
            try
            {
                var message = await _messageRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(m => m.ExternalMessageId == externalMessageId);

                if (message == null)
                {
                    _logger.LogWarning("Message not found for external ID: {ExternalMessageId}", externalMessageId);
                    return false;
                }

                // Update message with detailed status info
                message.Status = MapProviderStatusToMessageStatus(statusDto.Status);

                if (!string.IsNullOrEmpty(statusDto.ErrorMessage))
                {
                    message.ErrorMessage = statusDto.ErrorMessage;
                }

                if (statusDto.DeliveredAt.HasValue)
                {
                    message.DeliveredAt = statusDto.DeliveredAt.Value;
                }

                if (statusDto.FailedAt.HasValue)
                {
                    message.FailedAt = statusDto.FailedAt.Value;
                }

                if (statusDto.Cost.HasValue)
                {
                    message.CostAmount = statusDto.Cost.Value;
                }

                _messageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated message {MessageId} with delivery status: {Status}",
                    message.Id, statusDto.Status);

                // Trigger workflow events for delivery status changes
                if (message.ContactId > 0)
                {
                    var eventData = new Dictionary<string, object>
                    {
                        { "messageId", message.Id },
                        { "externalMessageId", externalMessageId },
                        { "status", statusDto.Status }
                    };

                    var mappedStatus = MapProviderStatusToMessageStatus(statusDto.Status);
                    if (mappedStatus == MessageStatus.Delivered)
                    {
                        await _eventTriggerService.TriggerEventAsync(EventType.MessageDelivered, message.ContactId, eventData);
                    }
                    else if (mappedStatus == MessageStatus.Failed)
                    {
                        eventData["errorMessage"] = statusDto.ErrorMessage ?? "";
                        await _eventTriggerService.TriggerEventAsync(EventType.MessageFailed, message.ContactId, eventData);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery status for {ExternalMessageId}", externalMessageId);
                return false;
            }
        }

        public bool ValidateWebhookSignature(string signature, string payload, string secret)
        {
            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToBase64String(hash);

                return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        private MessageStatus MapProviderStatusToMessageStatus(string providerStatus)
        {
            return providerStatus.ToLowerInvariant() switch
            {
                "queued" or "accepted" or "scheduled" => MessageStatus.Queued,
                "sending" or "sent" => MessageStatus.Sending,
                "delivered" => MessageStatus.Delivered,
                "failed" or "undelivered" => MessageStatus.Failed,
                "bounced" => MessageStatus.Failed,
                _ => MessageStatus.Queued
            };
        }
    }
}
