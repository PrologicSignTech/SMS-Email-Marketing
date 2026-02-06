using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Compliance;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MarketingPlatform.Application.Services
{
    public class ComplianceService : IComplianceService
    {
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactConsent> _consentRepository;
        private readonly IRepository<ConsentHistory> _consentHistoryRepository;
        private readonly IRepository<ComplianceSettings> _complianceSettingsRepository;
        private readonly IRepository<ComplianceAuditLog> _auditLogRepository;
        private readonly IRepository<SuppressionList> _suppressionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(
            IRepository<Contact> contactRepository,
            IRepository<ContactConsent> consentRepository,
            IRepository<ConsentHistory> consentHistoryRepository,
            IRepository<ComplianceSettings> complianceSettingsRepository,
            IRepository<ComplianceAuditLog> auditLogRepository,
            IRepository<SuppressionList> suppressionRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ComplianceService> logger)
        {
            _contactRepository = contactRepository;
            _consentRepository = consentRepository;
            _consentHistoryRepository = consentHistoryRepository;
            _complianceSettingsRepository = complianceSettingsRepository;
            _auditLogRepository = auditLogRepository;
            _suppressionRepository = suppressionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Consent Management

        public async Task<ConsentStatusDto?> GetContactConsentStatusAsync(string userId, int contactId)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return null;

            return new ConsentStatusDto
            {
                ContactId = contact.Id,
                ContactName = $"{contact.FirstName} {contact.LastName}".Trim(),
                PhoneNumber = contact.PhoneNumber,
                Email = contact.Email,
                SmsOptIn = contact.SmsOptIn,
                MmsOptIn = contact.MmsOptIn,
                EmailOptIn = contact.EmailOptIn,
                SmsOptInDate = contact.SmsOptInDate,
                MmsOptInDate = contact.MmsOptInDate,
                EmailOptInDate = contact.EmailOptInDate
            };
        }

        public async Task<ContactConsentDto> RecordConsentAsync(string userId, ConsentRequestDto request)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == request.ContactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                throw new InvalidOperationException("Contact not found");

            // Update contact opt-in flags
            UpdateContactOptInFlags(contact, request.Channel, request.ConsentGiven);
            _contactRepository.Update(contact);

            // Create consent record
            var consent = new ContactConsent
            {
                ContactId = request.ContactId,
                Channel = request.Channel,
                Status = request.ConsentGiven ? ConsentStatus.OptedIn : ConsentStatus.OptedOut,
                Source = request.Source,
                ConsentDate = DateTime.UtcNow,
                RevokedDate = request.ConsentGiven ? null : DateTime.UtcNow,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Notes = request.Notes
            };

            await _consentRepository.AddAsync(consent);

            // Create consent history
            var history = new ConsentHistory
            {
                ContactId = request.ContactId,
                ConsentGiven = request.ConsentGiven,
                ConsentType = request.ConsentGiven ? "Opt-In" : "Opt-Out",
                Channel = request.Channel,
                Source = request.Source,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                ConsentDate = DateTime.UtcNow
            };

            await _consentHistoryRepository.AddAsync(history);

            // Log audit trail
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            if (settings.EnableAuditLogging)
            {
                await LogComplianceActionAsync(
                    userId,
                    request.ConsentGiven ? ComplianceActionType.OptIn : ComplianceActionType.OptOut,
                    request.Channel,
                    request.ContactId,
                    null,
                    $"Contact {(request.ConsentGiven ? "opted in" : "opted out")} for {request.Channel}",
                    request.IpAddress
                );
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ContactConsentDto>(consent);
        }

        public async Task<int> BulkRecordConsentAsync(string userId, BulkConsentRequestDto request)
        {
            int count = 0;

            foreach (var contactId in request.ContactIds)
            {
                try
                {
                    var individualRequest = new ConsentRequestDto
                    {
                        ContactId = contactId,
                        Channel = request.Channel,
                        Source = request.Source,
                        ConsentGiven = request.ConsentGiven,
                        IpAddress = request.IpAddress,
                        Notes = request.Notes
                    };

                    await RecordConsentAsync(userId, individualRequest);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record consent for contact {ContactId}", contactId);
                }
            }

            return count;
        }

        public async Task<bool> RevokeConsentAsync(string userId, int contactId, ConsentChannel channel, string? reason = null)
        {
            var request = new ConsentRequestDto
            {
                ContactId = contactId,
                Channel = channel,
                Source = ConsentSource.Manual,
                ConsentGiven = false,
                Notes = reason
            };

            await RecordConsentAsync(userId, request);
            return true;
        }

        public async Task<PaginatedResult<ContactConsentDto>> GetContactConsentsAsync(string userId, int contactId, PagedRequest request)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                throw new InvalidOperationException("Contact not found");

            var query = (await _consentRepository.FindAsync(c =>
                c.ContactId == contactId && !c.IsDeleted)).AsQueryable();

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(c => c.ConsentDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<ContactConsentDto>>(items);

            return new PaginatedResult<ContactConsentDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<PaginatedResult<ConsentHistoryDto>> GetConsentHistoryAsync(string userId, int contactId, PagedRequest request)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                throw new InvalidOperationException("Contact not found");

            var query = (await _consentHistoryRepository.FindAsync(h =>
                h.ContactId == contactId && !h.IsDeleted)).AsQueryable();

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(h => h.ConsentDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<ConsentHistoryDto>>(items);

            return new PaginatedResult<ConsentHistoryDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        #endregion

        #region Compliance Settings

        public async Task<ComplianceSettingsDto> GetComplianceSettingsAsync(string userId)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            return _mapper.Map<ComplianceSettingsDto>(settings);
        }

        public async Task<ComplianceSettingsDto> UpdateComplianceSettingsAsync(string userId, UpdateComplianceSettingsDto dto)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);

            // Update settings
            settings.RequireDoubleOptIn = dto.RequireDoubleOptIn;
            settings.RequireDoubleOptInSms = dto.RequireDoubleOptInSms;
            settings.RequireDoubleOptInEmail = dto.RequireDoubleOptInEmail;
            settings.EnableQuietHours = dto.EnableQuietHours;
            settings.QuietHoursStart = dto.QuietHoursStart;
            settings.QuietHoursEnd = dto.QuietHoursEnd;
            settings.QuietHoursTimeZone = dto.QuietHoursTimeZone;
            settings.CompanyName = dto.CompanyName;
            settings.CompanyAddress = dto.CompanyAddress;
            settings.PrivacyPolicyUrl = dto.PrivacyPolicyUrl;
            settings.TermsOfServiceUrl = dto.TermsOfServiceUrl;
            settings.OptOutKeywords = dto.OptOutKeywords;
            settings.OptInKeywords = dto.OptInKeywords;
            settings.OptOutConfirmationMessage = dto.OptOutConfirmationMessage;
            settings.OptInConfirmationMessage = dto.OptInConfirmationMessage;
            settings.EnforceSuppressionList = dto.EnforceSuppressionList;
            settings.EnableConsentTracking = dto.EnableConsentTracking;
            settings.EnableAuditLogging = dto.EnableAuditLogging;
            settings.ConsentRetentionDays = dto.ConsentRetentionDays;
            settings.UpdatedAt = DateTime.UtcNow;

            _complianceSettingsRepository.Update(settings);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ComplianceSettingsDto>(settings);
        }

        public async Task<ComplianceSettingsDto> CreateDefaultComplianceSettingsAsync(string userId)
        {
            var settings = new ComplianceSettings
            {
                UserId = userId,
                RequireDoubleOptIn = false,
                RequireDoubleOptInSms = false,
                RequireDoubleOptInEmail = false,
                EnableQuietHours = false,
                EnforceSuppressionList = true,
                EnableConsentTracking = true,
                EnableAuditLogging = true,
                ConsentRetentionDays = 2555,
                OptOutKeywords = "STOP,UNSUBSCRIBE,CANCEL,END,QUIT",
                OptInKeywords = "START,SUBSCRIBE,YES,JOIN",
                OptOutConfirmationMessage = "You have been unsubscribed. Reply START to opt back in.",
                OptInConfirmationMessage = "You have been subscribed. Reply STOP to opt out."
            };

            await _complianceSettingsRepository.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ComplianceSettingsDto>(settings);
        }

        #endregion

        #region Compliance Checks

        public async Task<ComplianceCheckResultDto> CheckComplianceAsync(string userId, int contactId, ConsentChannel channel, int? campaignId = null)
        {
            var result = new ComplianceCheckResultDto
            {
                IsCompliant = true,
                Violations = new List<string>()
            };

            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
            {
                result.IsCompliant = false;
                result.Violations.Add("Contact not found");
                return result;
            }

            // Check consent
            result.HasConsent = CheckContactConsent(contact, channel);
            if (!result.HasConsent && settings.EnableConsentTracking)
            {
                result.IsCompliant = false;
                result.Violations.Add($"Contact has not opted in for {channel}");
            }

            // Check suppression list
            if (settings.EnforceSuppressionList)
            {
                var checkPhone = await _suppressionRepository.FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.PhoneOrEmail.ToLower() == contact.PhoneNumber.ToLower() &&
                    !s.IsDeleted);

                var checkEmail = !string.IsNullOrEmpty(contact.Email)
                    ? await _suppressionRepository.FirstOrDefaultAsync(s =>
                        s.UserId == userId &&
                        s.PhoneOrEmail.ToLower() == contact.Email.ToLower() &&
                        !s.IsDeleted)
                    : null;

                result.IsSuppressed = checkPhone != null || checkEmail != null;
                if (result.IsSuppressed)
                {
                    result.IsCompliant = false;
                    result.Violations.Add("Contact is on the suppression list");
                }
            }

            // Check quiet hours
            var quietHoursCheck = await CheckQuietHoursAsync(userId);
            result.IsQuietHoursViolation = quietHoursCheck.IsQuietHours;
            if (result.IsQuietHoursViolation)
            {
                result.IsCompliant = false;
                result.Violations.Add($"Current time is within quiet hours ({quietHoursCheck.QuietHoursStart} - {quietHoursCheck.QuietHoursEnd})");
            }

            // Log compliance check
            if (settings.EnableAuditLogging)
            {
                await LogComplianceActionAsync(
                    userId,
                    ComplianceActionType.ComplianceCheck,
                    channel,
                    contactId,
                    campaignId,
                    $"Compliance check result: {(result.IsCompliant ? "Compliant" : "Non-compliant")} - {string.Join(", ", result.Violations)}"
                );
            }

            result.Message = result.IsCompliant
                ? "Contact is compliant for messaging"
                : $"Compliance violations: {string.Join("; ", result.Violations)}";

            return result;
        }

        public async Task<QuietHoursCheckDto> CheckQuietHoursAsync(string userId, DateTime? sendTime = null)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            var result = new QuietHoursCheckDto
            {
                IsQuietHours = false,
                QuietHoursStart = settings.QuietHoursStart,
                QuietHoursEnd = settings.QuietHoursEnd
            };

            if (!settings.EnableQuietHours || !settings.QuietHoursStart.HasValue || !settings.QuietHoursEnd.HasValue)
            {
                result.Message = "Quiet hours not enabled";
                return result;
            }

            var checkTime = sendTime ?? DateTime.UtcNow;
            
            // Convert to user's timezone if specified
            if (!string.IsNullOrEmpty(settings.QuietHoursTimeZone))
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.QuietHoursTimeZone);
                    checkTime = TimeZoneInfo.ConvertTimeFromUtc(checkTime, timeZone);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid timezone: {TimeZone}", settings.QuietHoursTimeZone);
                }
            }

            var currentTime = checkTime.TimeOfDay;
            var start = settings.QuietHoursStart.Value;
            var end = settings.QuietHoursEnd.Value;

            // Handle quiet hours that span midnight
            if (start < end)
            {
                result.IsQuietHours = currentTime >= start && currentTime < end;
            }
            else
            {
                result.IsQuietHours = currentTime >= start || currentTime < end;
            }

            if (result.IsQuietHours)
            {
                result.Message = $"Current time {currentTime:hh\\:mm} is within quiet hours ({start:hh\\:mm} - {end:hh\\:mm})";
                
                // Calculate next allowed time
                if (start < end)
                {
                    result.NextAllowedTime = checkTime.Date.Add(end);
                }
                else
                {
                    result.NextAllowedTime = currentTime < end 
                        ? checkTime.Date.Add(end) 
                        : checkTime.Date.AddDays(1).Add(end);
                }
            }
            else
            {
                result.Message = "Not in quiet hours";
            }

            return result;
        }

        public async Task<bool> IsContactSuppressedAsync(string userId, string phoneOrEmail, ConsentChannel? channel = null)
        {
            var suppression = await _suppressionRepository.FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.PhoneOrEmail.ToLower() == phoneOrEmail.ToLower() &&
                !s.IsDeleted);

            return suppression != null;
        }

        public async Task<List<int>> FilterCompliantContactsAsync(string userId, List<int> contactIds, ConsentChannel channel, int? campaignId = null)
        {
            var compliantContacts = new List<int>();

            foreach (var contactId in contactIds)
            {
                var check = await CheckComplianceAsync(userId, contactId, channel, campaignId);
                if (check.IsCompliant)
                {
                    compliantContacts.Add(contactId);
                }
            }

            return compliantContacts;
        }

        #endregion

        #region Audit Logging

        public async Task LogComplianceActionAsync(string userId, ComplianceActionType actionType, ConsentChannel channel,
            int? contactId = null, int? campaignId = null, string? description = null, string? ipAddress = null)
        {
            var log = new ComplianceAuditLog
            {
                UserId = userId,
                ContactId = contactId,
                CampaignId = campaignId,
                ActionType = actionType,
                Channel = channel,
                Description = description,
                IpAddress = ipAddress,
                ActionDate = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResult<ComplianceAuditLogDto>> GetAuditLogsAsync(string userId, PagedRequest request,
            ComplianceActionType? actionType = null, ConsentChannel? channel = null, int? contactId = null)
        {
            var query = (await _auditLogRepository.FindAsync(l =>
                l.UserId == userId && !l.IsDeleted)).AsQueryable();

            // Apply filters
            if (actionType.HasValue)
                query = query.Where(l => l.ActionType == actionType.Value);

            if (channel.HasValue)
                query = query.Where(l => l.Channel == channel.Value);

            if (contactId.HasValue)
                query = query.Where(l => l.ContactId == contactId.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(l => l.ActionDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<ComplianceAuditLogDto>>(items);

            return new PaginatedResult<ComplianceAuditLogDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        #endregion

        #region Opt-In/Opt-Out Automation

        public async Task<bool> ProcessOptOutKeywordAsync(string userId, int contactId, string keyword, ConsentChannel channel)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            var keywords = settings.OptOutKeywords?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToUpper()).ToList() ?? new List<string>();

            if (keywords.Contains(keyword.ToUpper()))
            {
                await RevokeConsentAsync(userId, contactId, channel, $"Opted out via keyword: {keyword}");
                
                // Add to suppression list
                var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                    c.Id == contactId && c.UserId == userId && !c.IsDeleted);

                if (contact != null)
                {
                    var identifier = channel == ConsentChannel.Email ? contact.Email : contact.PhoneNumber;
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        var existing = await _suppressionRepository.FirstOrDefaultAsync(s =>
                            s.UserId == userId &&
                            s.PhoneOrEmail.ToLower() == identifier.ToLower() &&
                            !s.IsDeleted);

                        if (existing == null)
                        {
                            var suppression = new SuppressionList
                            {
                                UserId = userId,
                                PhoneOrEmail = identifier,
                                Type = SuppressionType.OptOut,
                                Reason = $"Opted out via keyword: {keyword}"
                            };
                            await _suppressionRepository.AddAsync(suppression);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public async Task<bool> ProcessOptInKeywordAsync(string userId, int contactId, string keyword, ConsentChannel channel)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            var keywords = settings.OptInKeywords?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToUpper()).ToList() ?? new List<string>();

            if (keywords.Contains(keyword.ToUpper()))
            {
                var request = new ConsentRequestDto
                {
                    ContactId = contactId,
                    Channel = channel,
                    Source = ConsentSource.Keyword,
                    ConsentGiven = true,
                    Notes = $"Opted in via keyword: {keyword}"
                };

                await RecordConsentAsync(userId, request);

                // Remove from suppression list if present
                var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                    c.Id == contactId && c.UserId == userId && !c.IsDeleted);

                if (contact != null)
                {
                    var identifier = channel == ConsentChannel.Email ? contact.Email : contact.PhoneNumber;
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        var suppression = await _suppressionRepository.FirstOrDefaultAsync(s =>
                            s.UserId == userId &&
                            s.PhoneOrEmail.ToLower() == identifier.ToLower() &&
                            !s.IsDeleted);

                        if (suppression != null)
                        {
                            suppression.IsDeleted = true;
                            suppression.UpdatedAt = DateTime.UtcNow;
                            _suppressionRepository.Update(suppression);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public async Task<string?> GetOptOutConfirmationMessageAsync(string userId)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            return settings.OptOutConfirmationMessage;
        }

        public async Task<string?> GetOptInConfirmationMessageAsync(string userId)
        {
            var settings = await GetOrCreateComplianceSettingsAsync(userId);
            return settings.OptInConfirmationMessage;
        }

        #endregion

        #region Private Helper Methods

        private async Task<ComplianceSettings> GetOrCreateComplianceSettingsAsync(string userId)
        {
            var settings = await _complianceSettingsRepository.FirstOrDefaultAsync(s =>
                s.UserId == userId && !s.IsDeleted);

            if (settings == null)
            {
                settings = new ComplianceSettings
                {
                    UserId = userId,
                    RequireDoubleOptIn = false,
                    RequireDoubleOptInSms = false,
                    RequireDoubleOptInEmail = false,
                    EnableQuietHours = false,
                    EnforceSuppressionList = true,
                    EnableConsentTracking = true,
                    EnableAuditLogging = true,
                    ConsentRetentionDays = 2555,
                    OptOutKeywords = "STOP,UNSUBSCRIBE,CANCEL,END,QUIT",
                    OptInKeywords = "START,SUBSCRIBE,YES,JOIN",
                    OptOutConfirmationMessage = "You have been unsubscribed. Reply START to opt back in.",
                    OptInConfirmationMessage = "You have been subscribed. Reply STOP to opt out."
                };

                await _complianceSettingsRepository.AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }

            return settings;
        }

        private void UpdateContactOptInFlags(Contact contact, ConsentChannel channel, bool optIn)
        {
            var now = DateTime.UtcNow;

            switch (channel)
            {
                case ConsentChannel.SMS:
                    contact.SmsOptIn = optIn;
                    contact.SmsOptInDate = optIn ? now : null;
                    break;
                case ConsentChannel.MMS:
                    contact.MmsOptIn = optIn;
                    contact.MmsOptInDate = optIn ? now : null;
                    break;
                case ConsentChannel.Email:
                    contact.EmailOptIn = optIn;
                    contact.EmailOptInDate = optIn ? now : null;
                    break;
                case ConsentChannel.All:
                    contact.SmsOptIn = optIn;
                    contact.MmsOptIn = optIn;
                    contact.EmailOptIn = optIn;
                    contact.SmsOptInDate = optIn ? now : null;
                    contact.MmsOptInDate = optIn ? now : null;
                    contact.EmailOptInDate = optIn ? now : null;
                    break;
            }

            contact.UpdatedAt = now;
        }

        private bool CheckContactConsent(Contact contact, ConsentChannel channel)
        {
            return channel switch
            {
                ConsentChannel.SMS => contact.SmsOptIn,
                ConsentChannel.MMS => contact.MmsOptIn,
                ConsentChannel.Email => contact.EmailOptIn,
                ConsentChannel.All => contact.SmsOptIn && contact.MmsOptIn && contact.EmailOptIn,
                _ => false
            };
        }

        #endregion
    }
}
