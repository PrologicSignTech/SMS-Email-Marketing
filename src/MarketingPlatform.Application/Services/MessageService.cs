using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class MessageService : IMessageService
    {
        // Constants for test messages
        private const string TestMessagePrefix = "[TEST]";
        private const string TestMessageBodyPrefix = "[TEST MESSAGE]\n\n";

        // SMS/MMS character limits
        private const int SmsGsm7SingleSegment = 160;
        private const int SmsGsm7ConcatenatedSegment = 153;
        private const int SmsUnicodeSingleSegment = 70;
        private const int SmsUnicodeConcatenatedSegment = 67;

        // Unicode special characters not in GSM-7 basic set
        private static readonly char[] UnicodeSpecialChars = { '€', '[', ']', '{', '}', '\\', '^', '~', '|' };

        // Regex for variable extraction
        private static readonly System.Text.RegularExpressions.Regex VariableRegex = 
            new System.Text.RegularExpressions.Regex(@"\{\{([^}]+)\}\}", System.Text.RegularExpressions.RegexOptions.Compiled);

        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISMSProvider _smsProvider;
        private readonly IMMSProvider _mmsProvider;
        private readonly IEmailProvider _emailProvider;
        private readonly IMessageRoutingService _routingService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IRepository<CampaignMessage> messageRepository,
            IRepository<Contact> contactRepository,
            IRepository<Campaign> campaignRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISMSProvider smsProvider,
            IMMSProvider mmsProvider,
            IEmailProvider emailProvider,
            IMessageRoutingService routingService,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _contactRepository = contactRepository;
            _campaignRepository = campaignRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _smsProvider = smsProvider;
            _mmsProvider = mmsProvider;
            _emailProvider = emailProvider;
            _routingService = routingService;
            _logger = logger;
        }

        public async Task<MessageDto?> GetMessageByIdAsync(string userId, int messageId)
        {
            var message = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .Where(m => m.Id == messageId && m.Campaign.UserId == userId)
                .FirstOrDefaultAsync();

            if (message == null)
                return null;

            var dto = _mapper.Map<MessageDto>(message);
            
            // Deserialize MediaUrls if present
            if (!string.IsNullOrEmpty(message.MediaUrls))
            {
                dto.MediaUrls = JsonConvert.DeserializeObject<List<string>>(message.MediaUrls);
            }

            return dto;
        }

        public async Task<PaginatedResult<MessageDto>> GetMessagesAsync(string userId, PagedRequest request)
        {
            var query = _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .Where(m => m.Campaign.UserId == userId);

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(m => 
                    m.Recipient.Contains(request.SearchTerm) ||
                    m.Campaign.Name.Contains(request.SearchTerm) ||
                    m.Contact.FirstName!.Contains(request.SearchTerm) ||
                    m.Contact.LastName!.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            query = request.SortDescending
                ? query.OrderByDescending(m => EF.Property<object>(m, request.SortBy ?? "CreatedAt"))
                : query.OrderBy(m => EF.Property<object>(m, request.SortBy ?? "CreatedAt"));

            var messages = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = messages.Select(m => {
                var dto = _mapper.Map<MessageDto>(m);
                if (!string.IsNullOrEmpty(m.MediaUrls))
                {
                    dto.MediaUrls = JsonConvert.DeserializeObject<List<string>>(m.MediaUrls);
                }
                return dto;
            }).ToList();

            return new PaginatedResult<MessageDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<PaginatedResult<MessageDto>> GetMessagesByCampaignAsync(string userId, int campaignId, PagedRequest request)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null || campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Campaign not found or access denied");
            }

            var query = _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .Where(m => m.CampaignId == campaignId);

            var totalCount = await query.CountAsync();

            query = request.SortDescending
                ? query.OrderByDescending(m => EF.Property<object>(m, request.SortBy ?? "CreatedAt"))
                : query.OrderBy(m => EF.Property<object>(m, request.SortBy ?? "CreatedAt"));

            var messages = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = messages.Select(m => {
                var dto = _mapper.Map<MessageDto>(m);
                if (!string.IsNullOrEmpty(m.MediaUrls))
                {
                    dto.MediaUrls = JsonConvert.DeserializeObject<List<string>>(m.MediaUrls);
                }
                return dto;
            }).ToList();

            return new PaginatedResult<MessageDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<List<MessageDto>> GetMessagesByStatusAsync(string userId, MessageStatus status)
        {
            var messages = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .Where(m => m.Campaign.UserId == userId && m.Status == status)
                .ToListAsync();

            return messages.Select(m => {
                var dto = _mapper.Map<MessageDto>(m);
                if (!string.IsNullOrEmpty(m.MediaUrls))
                {
                    dto.MediaUrls = JsonConvert.DeserializeObject<List<string>>(m.MediaUrls);
                }
                return dto;
            }).ToList();
        }

        public async Task<MessageDto> CreateMessageAsync(string userId, CreateMessageDto dto)
        {
            var campaign = await _campaignRepository.GetByIdAsync(dto.CampaignId);
            if (campaign == null || campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Campaign not found or access denied");
            }

            var contact = await _contactRepository.GetByIdAsync(dto.ContactId);
            if (contact == null || contact.UserId != userId)
            {
                throw new UnauthorizedAccessException("Contact not found or access denied");
            }

            var message = _mapper.Map<CampaignMessage>(dto);
            message.Status = MessageStatus.Queued;
            
            if (dto.MediaUrls != null && dto.MediaUrls.Any())
            {
                message.MediaUrls = JsonConvert.SerializeObject(dto.MediaUrls);
            }

            await _messageRepository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            var createdMessage = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .FirstAsync(m => m.Id == message.Id);

            var result = _mapper.Map<MessageDto>(createdMessage);
            if (!string.IsNullOrEmpty(createdMessage.MediaUrls))
            {
                result.MediaUrls = JsonConvert.DeserializeObject<List<string>>(createdMessage.MediaUrls);
            }

            return result;
        }

        public async Task<List<MessageDto>> CreateBulkMessagesAsync(string userId, BulkMessageRequestDto dto)
        {
            var campaign = await _campaignRepository.GetByIdAsync(dto.CampaignId);
            if (campaign == null || campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Campaign not found or access denied");
            }

            var contacts = await _contactRepository.GetQueryable()
                .Where(c => dto.ContactIds.Contains(c.Id) && c.UserId == userId)
                .ToListAsync();

            if (contacts.Count != dto.ContactIds.Count)
            {
                throw new ArgumentException("Some contacts were not found or access denied");
            }

            var messages = new List<CampaignMessage>();
            var mediaUrlsJson = dto.MediaUrls != null && dto.MediaUrls.Any() 
                ? JsonConvert.SerializeObject(dto.MediaUrls) 
                : null;

            foreach (var contact in contacts)
            {
                var recipient = dto.Channel == ChannelType.Email 
                    ? contact.Email ?? contact.PhoneNumber 
                    : contact.PhoneNumber;

                var message = new CampaignMessage
                {
                    CampaignId = dto.CampaignId,
                    ContactId = contact.Id,
                    Recipient = recipient,
                    Channel = dto.Channel,
                    Subject = dto.Subject,
                    MessageBody = dto.MessageBody,
                    HTMLContent = dto.HTMLContent,
                    MediaUrls = mediaUrlsJson,
                    ScheduledAt = dto.ScheduledAt,
                    Status = MessageStatus.Queued,
                    MaxRetries = 3
                };

                messages.Add(message);
            }

            foreach (var message in messages)
            {
                await _messageRepository.AddAsync(message);
            }
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            var messageIds = messages.Select(m => m.Id).ToList();
            var createdMessages = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .Include(m => m.Contact)
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            return createdMessages.Select(m => {
                var result = _mapper.Map<MessageDto>(m);
                if (!string.IsNullOrEmpty(m.MediaUrls))
                {
                    result.MediaUrls = JsonConvert.DeserializeObject<List<string>>(m.MediaUrls);
                }
                return result;
            }).ToList();
        }

        public async Task<bool> UpdateMessageStatusAsync(string userId, int messageId, MessageStatusUpdateDto dto)
        {
            var message = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return false;

            // For webhook updates, userId can be system (empty) or match the campaign owner
            if (!string.IsNullOrEmpty(userId) && message.Campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            message.Status = dto.Status;
            message.ErrorMessage = dto.ErrorMessage;

            if (dto.CostAmount.HasValue)
            {
                message.CostAmount = dto.CostAmount.Value;
            }

            if (dto.Status == MessageStatus.Delivered)
            {
                message.DeliveredAt = DateTime.UtcNow;
            }
            else if (dto.Status == MessageStatus.Failed || dto.Status == MessageStatus.Bounced)
            {
                message.FailedAt = DateTime.UtcNow;
            }

            _messageRepository.Update(message);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RetryFailedMessageAsync(string userId, int messageId)
        {
            var message = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.Campaign.UserId == userId);

            if (message == null)
                return false;

            if (message.Status != MessageStatus.Failed && message.Status != MessageStatus.Bounced)
            {
                throw new InvalidOperationException("Only failed or bounced messages can be retried");
            }

            if (message.RetryCount >= message.MaxRetries)
            {
                throw new InvalidOperationException("Maximum retry attempts reached");
            }

            message.Status = MessageStatus.Queued;
            message.ErrorMessage = null;
            message.ScheduledAt = DateTime.UtcNow;
            message.RetryCount++;

            _messageRepository.Update(message);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> RetryFailedMessagesForCampaignAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null || campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Campaign not found or access denied");
            }

            var failedMessages = await _messageRepository.GetQueryable()
                .Where(m => m.CampaignId == campaignId && 
                           (m.Status == MessageStatus.Failed || m.Status == MessageStatus.Bounced) &&
                           m.RetryCount < m.MaxRetries)
                .ToListAsync();

            foreach (var message in failedMessages)
            {
                message.Status = MessageStatus.Queued;
                message.ErrorMessage = null;
                message.ScheduledAt = DateTime.UtcNow;
                message.RetryCount++;
                _messageRepository.Update(message);
            }

            await _unitOfWork.SaveChangesAsync();

            return failedMessages.Count;
        }

        public async Task<bool> CancelScheduledMessageAsync(string userId, int messageId)
        {
            var message = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.Campaign.UserId == userId);

            if (message == null)
                return false;

            if (message.Status != MessageStatus.Queued)
            {
                throw new InvalidOperationException("Only queued messages can be cancelled");
            }

            // Instead of deleting, mark as failed with cancellation message
            message.Status = MessageStatus.Failed;
            message.ErrorMessage = "Cancelled by user";
            message.FailedAt = DateTime.UtcNow;

            _messageRepository.Update(message);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<MessageDeliveryReportDto> GetDeliveryReportAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null || campaign.UserId != userId)
            {
                throw new UnauthorizedAccessException("Campaign not found or access denied");
            }

            var messages = await _messageRepository.GetQueryable()
                .Where(m => m.CampaignId == campaignId)
                .ToListAsync();

            var totalMessages = messages.Count;
            var queued = messages.Count(m => m.Status == MessageStatus.Queued);
            var sending = messages.Count(m => m.Status == MessageStatus.Sending);
            var sent = messages.Count(m => m.Status == MessageStatus.Sent);
            var delivered = messages.Count(m => m.Status == MessageStatus.Delivered);
            var failed = messages.Count(m => m.Status == MessageStatus.Failed);
            var bounced = messages.Count(m => m.Status == MessageStatus.Bounced);
            var totalCost = messages.Sum(m => m.CostAmount);

            var deliveryRate = totalMessages > 0 
                ? (decimal)delivered / totalMessages * 100 
                : 0;

            var failureRate = totalMessages > 0 
                ? (decimal)(failed + bounced) / totalMessages * 100 
                : 0;

            var averageCost = totalMessages > 0 
                ? totalCost / totalMessages 
                : 0;

            return new MessageDeliveryReportDto
            {
                CampaignId = campaignId,
                TotalMessages = totalMessages,
                Queued = queued,
                Sending = sending,
                Sent = sent,
                Delivered = delivered,
                Failed = failed,
                Bounced = bounced,
                DeliveryRate = Math.Round(deliveryRate, 2),
                FailureRate = Math.Round(failureRate, 2),
                TotalCost = totalCost,
                AverageCost = Math.Round(averageCost, 4)
            };
        }

        public async Task ProcessMessageQueueAsync()
        {
            // Use AsNoTracking for read query to avoid locks and improve performance
            // Take only a batch of messages at a time to prevent long-running transactions
            var messagesToProcess = await _messageRepository.GetQueryable()
                .AsNoTracking()
                .Where(m => m.Status == MessageStatus.Queued && 
                           (m.ScheduledAt == null || m.ScheduledAt <= DateTime.UtcNow))
                .Take(50) // Process max 50 messages per batch
                .ToListAsync();

            if (!messagesToProcess.Any())
                return;

            _logger.LogInformation("Processing {Count} messages from queue", messagesToProcess.Count);

            foreach (var msg in messagesToProcess)
            {
                try
                {
                    // Update status to Sending in a separate transaction to release locks quickly
                    await UpdateMessageStatusToSendingAsync(msg.Id);

                    // Use routing service for intelligent routing, fallback, and retry logic
                    var (success, externalId, error, cost, attemptNumber) = await _routingService.RouteMessageAsync(msg);

                    // Update final status in another separate transaction
                    await UpdateMessageAfterSendAsync(msg.Id, success, externalId, error, cost);

                    // If failed, check if should retry
                    if (!success)
                    {
                        var (shouldRetry, delaySeconds) = await _routingService.ShouldRetryMessageAsync(msg);
                        
                        if (shouldRetry && delaySeconds > 0)
                        {
                            // Schedule retry
                            var retryMessage = await _messageRepository.GetByIdAsync(msg.Id);
                            if (retryMessage != null)
                            {
                                retryMessage.ScheduledAt = DateTime.UtcNow.AddSeconds(delaySeconds);
                                retryMessage.Status = MessageStatus.Queued;
                                retryMessage.RetryCount++;
                                _messageRepository.Update(retryMessage);
                                await _unitOfWork.SaveChangesAsync();

                                _logger.LogInformation(
                                    "Message {MessageId} scheduled for retry in {DelaySeconds}s (Attempt {AttemptNumber})",
                                    msg.Id, delaySeconds, retryMessage.RetryCount + 1);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message {MessageId}", msg.Id);
                    await UpdateMessageAfterSendAsync(msg.Id, false, null, ex.Message, null);
                }
            }
        }

        private async Task UpdateMessageStatusToSendingAsync(int messageId)
        {
            // Quick update in a short transaction - no locks held during external API calls
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message != null && message.Status == MessageStatus.Queued)
            {
                message.Status = MessageStatus.Sending;
                _messageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task UpdateMessageAfterSendAsync(int messageId, bool success, string? externalId, string? error, decimal? cost)
        {
            // Quick update in a short transaction
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message != null)
            {
                if (success)
                {
                    message.Status = MessageStatus.Sent;
                    message.SentAt = DateTime.UtcNow;
                    message.ExternalMessageId = externalId;
                    message.ProviderMessageId = externalId; // Backward compatibility
                    if (cost.HasValue)
                    {
                        message.CostAmount = cost.Value;
                    }
                }
                else
                {
                    message.Status = MessageStatus.Failed;
                    message.FailedAt = DateTime.UtcNow;
                    message.ErrorMessage = error;
                }

                _messageRepository.Update(message);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> SendMessageNowAsync(string userId, int messageId)
        {
            var message = await _messageRepository.GetQueryable()
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.Campaign.UserId == userId);

            if (message == null)
                return false;

            if (message.Status != MessageStatus.Queued)
            {
                throw new InvalidOperationException("Only queued messages can be sent immediately");
            }

            // Update scheduled time to now to be picked up by queue processor
            message.ScheduledAt = DateTime.UtcNow;
            _messageRepository.Update(message);
            await _unitOfWork.SaveChangesAsync();

            // Trigger immediate processing
            await ProcessSingleMessageAsync(messageId);

            return true;
        }

        private async Task ProcessSingleMessageAsync(int messageId)
        {
            var message = await _messageRepository.GetQueryable()
                .AsNoTracking()
                .Where(m => m.Id == messageId)
                .FirstOrDefaultAsync();

            if (message == null || message.Status != MessageStatus.Queued)
                return;

            try
            {
                await UpdateMessageStatusToSendingAsync(message.Id);

                // Use routing service for intelligent routing
                var (success, externalId, error, cost, attemptNumber) = await _routingService.RouteMessageAsync(message);

                await UpdateMessageAfterSendAsync(message.Id, success, externalId, error, cost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                await UpdateMessageAfterSendAsync(message.Id, false, null, ex.Message, null);
            }
        }

        public async Task<MessagePreviewDto> PreviewMessageAsync(string userId, MessagePreviewRequestDto request)
        {
            // Validate campaign access if provided
            if (request.CampaignId.HasValue)
            {
                var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId.Value);
                if (campaign == null || campaign.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Campaign not found or access denied");
                }
            }

            // Build variable dictionary
            var variables = new Dictionary<string, string>(request.VariableValues, StringComparer.OrdinalIgnoreCase);

            // Load contact data if ContactId provided
            if (request.ContactId.HasValue)
            {
                var contact = await _contactRepository.GetByIdAsync(request.ContactId.Value);
                if (contact != null && contact.UserId == userId && !contact.IsDeleted)
                {
                    var contactVars = LoadContactVariables(contact);
                    foreach (var kvp in contactVars)
                    {
                        if (!variables.ContainsKey(kvp.Key))
                        {
                            variables[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            // Render content
            var renderedSubject = !string.IsNullOrWhiteSpace(request.Subject)
                ? RenderContent(request.Subject, variables)
                : null;
            var renderedBody = RenderContent(request.MessageBody, variables);
            var renderedHtml = !string.IsNullOrWhiteSpace(request.HTMLContent)
                ? RenderContent(request.HTMLContent, variables)
                : null;

            var preview = new MessagePreviewDto
            {
                Channel = request.Channel,
                Subject = renderedSubject,
                MessageBody = renderedBody,
                HTMLContent = renderedHtml,
                MediaUrls = request.MediaUrls,
                CharacterCount = renderedBody.Length
            };

            // Find missing variables
            preview.MissingVariables = FindMissingVariables(request.MessageBody, variables);
            if (!string.IsNullOrWhiteSpace(request.Subject))
            {
                preview.MissingVariables.AddRange(FindMissingVariables(request.Subject, variables));
            }
            if (!string.IsNullOrWhiteSpace(request.HTMLContent))
            {
                preview.MissingVariables.AddRange(FindMissingVariables(request.HTMLContent, variables));
            }
            preview.MissingVariables = preview.MissingVariables.Distinct().ToList();

            // Calculate SMS segments if applicable
            if (request.Channel == ChannelType.SMS || request.Channel == ChannelType.MMS)
            {
                preview.SmsSegments = CalculateSmsSegments(renderedBody);
            }

            // Validate content
            ValidateMessageContent(preview, request.Channel);

            // Generate device-specific previews
            preview.DevicePreviews = GenerateDevicePreviews(renderedSubject, renderedBody, renderedHtml, request.Channel);

            return preview;
        }

        public async Task<TestSendResultDto> SendTestMessageAsync(string userId, TestSendRequestDto request)
        {
            // Validate campaign access if provided
            if (request.CampaignId.HasValue)
            {
                var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId.Value);
                if (campaign == null || campaign.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Campaign not found or access denied");
                }
            }

            if (request.Recipients == null || !request.Recipients.Any())
            {
                throw new ArgumentException("At least one recipient is required for test send");
            }

            var result = new TestSendResultDto();

            // Build variable dictionary
            var variables = new Dictionary<string, string>(request.VariableValues, StringComparer.OrdinalIgnoreCase);

            // Load contact data if ContactId provided
            if (request.ContactId.HasValue)
            {
                var contact = await _contactRepository.GetByIdAsync(request.ContactId.Value);
                if (contact != null && contact.UserId == userId && !contact.IsDeleted)
                {
                    var contactVars = LoadContactVariables(contact);
                    foreach (var kvp in contactVars)
                    {
                        if (!variables.ContainsKey(kvp.Key))
                        {
                            variables[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            // Render content
            var renderedSubject = !string.IsNullOrWhiteSpace(request.Subject)
                ? RenderContent(request.Subject, variables)
                : null;
            var renderedBody = RenderContent(request.MessageBody, variables);
            var renderedHtml = !string.IsNullOrWhiteSpace(request.HTMLContent)
                ? RenderContent(request.HTMLContent, variables)
                : null;

            // Send test message to each recipient
            foreach (var recipient in request.Recipients)
            {
                var recipientResult = new TestSendRecipientResultDto
                {
                    Recipient = recipient
                };

                try
                {
                    string? externalId = null;
                    bool success = false;
                    string? errorMessage = null;

                    // Add test prefix to subject/body to indicate test message
                    var testSubject = renderedSubject != null ? $"{TestMessagePrefix} {renderedSubject}" : null;
                    var testBody = $"{TestMessageBodyPrefix}{renderedBody}";

                    switch (request.Channel)
                    {
                        case ChannelType.SMS:
                            var smsResult = await _smsProvider.SendSMSAsync(recipient, testBody);
                            success = smsResult.Success;
                            externalId = smsResult.ExternalId;
                            errorMessage = smsResult.Error;
                            break;

                        case ChannelType.MMS:
                            var mmsResult = await _mmsProvider.SendMMSAsync(
                                recipient, 
                                testBody, 
                                request.MediaUrls?.ToList() ?? new List<string>());
                            success = mmsResult.Success;
                            externalId = mmsResult.ExternalId;
                            errorMessage = mmsResult.Error;
                            break;

                        case ChannelType.Email:
                            var emailResult = await _emailProvider.SendEmailAsync(
                                recipient,
                                testSubject ?? TestMessagePrefix,
                                testBody,
                                renderedHtml);
                            success = emailResult.Success;
                            externalId = emailResult.ExternalId;
                            errorMessage = emailResult.Error;
                            break;
                    }

                    recipientResult.Success = success;
                    recipientResult.ExternalMessageId = externalId;
                    recipientResult.ErrorMessage = errorMessage;

                    if (success)
                    {
                        result.SuccessCount++;
                        _logger.LogInformation(
                            "Test message sent successfully to {Recipient} via {Channel}. ExternalId: {ExternalId}",
                            recipient, request.Channel, externalId);
                    }
                    else
                    {
                        result.FailureCount++;
                        _logger.LogWarning(
                            "Test message failed to send to {Recipient} via {Channel}. Error: {Error}",
                            recipient, request.Channel, errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    recipientResult.Success = false;
                    recipientResult.ErrorMessage = ex.Message;
                    result.FailureCount++;
                    _logger.LogError(ex, 
                        "Exception sending test message to {Recipient} via {Channel}",
                        recipient, request.Channel);
                }

                result.Recipients.Add(recipientResult);
            }

            return result;
        }

        // Private helper methods for preview and test send
        private Dictionary<string, string> LoadContactVariables(Contact contact)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FirstName", contact.FirstName ?? "" },
                { "LastName", contact.LastName ?? "" },
                { "Email", contact.Email ?? "" },
                { "Phone", contact.PhoneNumber ?? "" },
                { "PhoneNumber", contact.PhoneNumber ?? "" }
            };
        }

        private string RenderContent(string content, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(content) || variables.Count == 0)
                return content;

            var result = new System.Text.StringBuilder(content);

            foreach (var kvp in variables)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                result.Replace(placeholder, kvp.Value);
            }

            return result.ToString();
        }

        private List<string> FindMissingVariables(string content, Dictionary<string, string> providedVariables)
        {
            var allVariables = ExtractVariablesFromContent(content);
            return allVariables.Where(v => !providedVariables.ContainsKey(v)).ToList();
        }

        private List<string> ExtractVariablesFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var matches = VariableRegex.Matches(content);

            return matches
                .Select(m => m.Groups[1].Value.Trim())
                .Distinct()
                .ToList();
        }

        private int CalculateSmsSegments(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            var charCount = content.Length;
            var containsUnicode = ContainsUnicodeCharacters(content);

            if (containsUnicode)
            {
                return charCount <= SmsUnicodeSingleSegment
                    ? 1
                    : (int)Math.Ceiling((double)charCount / SmsUnicodeConcatenatedSegment);
            }
            else
            {
                return charCount <= SmsGsm7SingleSegment
                    ? 1
                    : (int)Math.Ceiling((double)charCount / SmsGsm7ConcatenatedSegment);
            }
        }

        private bool ContainsUnicodeCharacters(string text)
        {
            foreach (char c in text)
            {
                if (c > 127 || UnicodeSpecialChars.Contains(c))
                {
                    return true;
                }
            }
            return false;
        }

        private void ValidateMessageContent(MessagePreviewDto preview, ChannelType channel)
        {
            // Validate based on channel type
            switch (channel)
            {
                case ChannelType.SMS:
                    if (preview.CharacterCount > 1600)
                    {
                        preview.ValidationWarnings.Add("Message exceeds 10 SMS segments (1600 characters). Consider shortening.");
                    }
                    if (preview.SmsSegments > 1)
                    {
                        preview.ValidationWarnings.Add($"Message will be sent as {preview.SmsSegments} SMS segments.");
                    }
                    break;

                case ChannelType.MMS:
                    if (preview.CharacterCount > 1600)
                    {
                        preview.ValidationWarnings.Add("Message exceeds recommended length for MMS.");
                    }
                    if (preview.MediaUrls == null || !preview.MediaUrls.Any())
                    {
                        preview.ValidationWarnings.Add("MMS message has no media attachments.");
                    }
                    break;

                case ChannelType.Email:
                    if (!string.IsNullOrEmpty(preview.Subject) && preview.Subject.Length > 100)
                    {
                        preview.ValidationWarnings.Add("Subject line exceeds 100 characters. May be truncated in some email clients.");
                    }
                    if (string.IsNullOrEmpty(preview.Subject))
                    {
                        preview.ValidationWarnings.Add("Email has no subject line.");
                    }
                    if (string.IsNullOrEmpty(preview.HTMLContent) && preview.CharacterCount > 100000)
                    {
                        preview.ValidationWarnings.Add("Email body is very large. Consider using HTML format.");
                    }
                    break;
            }

            // Check for missing variables
            if (preview.MissingVariables.Any())
            {
                preview.ValidationWarnings.Add($"Missing variable values: {string.Join(", ", preview.MissingVariables)}");
            }
        }

        private List<DevicePreviewDto> GenerateDevicePreviews(
            string? subject, 
            string body, 
            string? htmlContent, 
            ChannelType channel)
        {
            var previews = new List<DevicePreviewDto>();

            if (channel == ChannelType.Email)
            {
                // Desktop preview
                previews.Add(new DevicePreviewDto
                {
                    DeviceType = "Desktop",
                    Subject = subject,
                    MessageBody = body,
                    HTMLContent = htmlContent,
                    CharacterCount = body.Length,
                    IsTruncated = false,
                    Warnings = new List<string>()
                });

                // Mobile preview - truncate subject if too long
                var mobileSubject = subject;
                var mobileWarnings = new List<string>();
                if (!string.IsNullOrEmpty(subject) && subject.Length > 40)
                {
                    mobileWarnings.Add("Subject may be truncated on mobile devices (typically shows ~40 characters)");
                }

                previews.Add(new DevicePreviewDto
                {
                    DeviceType = "Mobile",
                    Subject = mobileSubject,
                    MessageBody = body,
                    HTMLContent = htmlContent,
                    CharacterCount = body.Length,
                    IsTruncated = !string.IsNullOrEmpty(subject) && subject.Length > 40,
                    Warnings = mobileWarnings
                });

                // Tablet preview
                previews.Add(new DevicePreviewDto
                {
                    DeviceType = "Tablet",
                    Subject = subject,
                    MessageBody = body,
                    HTMLContent = htmlContent,
                    CharacterCount = body.Length,
                    IsTruncated = false,
                    Warnings = new List<string>()
                });
            }
            else // SMS/MMS
            {
                // Mobile is the primary device for SMS/MMS
                var warnings = new List<string>();
                var segments = CalculateSmsSegments(body);
                if (segments > 1)
                {
                    warnings.Add($"Message will be sent as {segments} segments");
                }

                previews.Add(new DevicePreviewDto
                {
                    DeviceType = "Mobile",
                    Subject = null,
                    MessageBody = body,
                    HTMLContent = null,
                    CharacterCount = body.Length,
                    IsTruncated = false,
                    Warnings = warnings
                });
            }

            return previews;
        }
    }
}
