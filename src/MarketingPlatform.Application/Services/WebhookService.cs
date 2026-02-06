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
        private readonly IKeywordService _keywordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(
            IRepository<CampaignMessage> messageRepository,
            IRepository<Contact> contactRepository,
            IRepository<SuppressionList> suppressionRepository,
            IKeywordService keywordService,
            IUnitOfWork unitOfWork,
            ILogger<WebhookService> logger)
        {
            _messageRepository = messageRepository;
            _contactRepository = contactRepository;
            _suppressionRepository = suppressionRepository;
            _keywordService = keywordService;
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
                _logger.LogInformation("Processing inbound message from {From}: {Body}", from, body);

                // Check for keywords and process
                await _keywordService.ProcessInboundKeywordAsync(from, body);

                // TODO: Store inbound message in database if needed for future reference
                // This could be useful for conversation tracking or compliance

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inbound message from {From}", from);
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
