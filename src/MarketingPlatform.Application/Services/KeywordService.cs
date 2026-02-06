using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Keyword;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class KeywordService : IKeywordService
    {
        private readonly IRepository<Keyword> _keywordRepository;
        private readonly IRepository<KeywordActivity> _keywordActivityRepository;
        private readonly IRepository<KeywordReservation> _keywordReservationRepository;
        private readonly IRepository<KeywordAssignment> _keywordAssignmentRepository;
        private readonly IRepository<KeywordConflict> _keywordConflictRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<ContactGroup> _contactGroupRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<KeywordService> _logger;
        private readonly ISMSProvider _smsProvider;

        public KeywordService(
            IRepository<Keyword> keywordRepository,
            IRepository<KeywordActivity> keywordActivityRepository,
            IRepository<KeywordReservation> keywordReservationRepository,
            IRepository<KeywordAssignment> keywordAssignmentRepository,
            IRepository<KeywordConflict> keywordConflictRepository,
            IRepository<Campaign> campaignRepository,
            IRepository<ContactGroup> contactGroupRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IRepository<Contact> contactRepository,
            IRepository<ApplicationUser> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<KeywordService> logger,
            ISMSProvider smsProvider)
        {
            _keywordRepository = keywordRepository;
            _keywordActivityRepository = keywordActivityRepository;
            _keywordReservationRepository = keywordReservationRepository;
            _keywordAssignmentRepository = keywordAssignmentRepository;
            _keywordConflictRepository = keywordConflictRepository;
            _campaignRepository = campaignRepository;
            _contactGroupRepository = contactGroupRepository;
            _groupMemberRepository = groupMemberRepository;
            _contactRepository = contactRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _smsProvider = smsProvider;
        }

        public async Task<KeywordDto?> GetKeywordByIdAsync(string userId, int keywordId)
        {
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == keywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
                return null;

            return await MapToKeywordDtoAsync(keyword);
        }

        public async Task<PaginatedResult<KeywordDto>> GetKeywordsAsync(string userId, PagedRequest request)
        {
            var query = (await _keywordRepository.FindAsync(k =>
                k.UserId == userId && !k.IsDeleted)).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(k =>
                    k.KeywordText.Contains(request.SearchTerm) ||
                    (k.Description != null && k.Description.Contains(request.SearchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "keywordtext" => request.SortDescending ? query.OrderByDescending(k => k.KeywordText) : query.OrderBy(k => k.KeywordText),
                    "status" => request.SortDescending ? query.OrderByDescending(k => k.Status) : query.OrderBy(k => k.Status),
                    "createdat" => request.SortDescending ? query.OrderByDescending(k => k.CreatedAt) : query.OrderBy(k => k.CreatedAt),
                    _ => request.SortDescending ? query.OrderByDescending(k => k.CreatedAt) : query.OrderBy(k => k.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(k => k.CreatedAt);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var keywords = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var keywordDtos = new List<KeywordDto>();
            foreach (var keyword in keywords)
            {
                keywordDtos.Add(await MapToKeywordDtoAsync(keyword));
            }

            return new PaginatedResult<KeywordDto>
            {
                Items = keywordDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<List<KeywordDto>> GetKeywordsByStatusAsync(string userId, KeywordStatus status)
        {
            var keywords = await _keywordRepository.FindAsync(k =>
                k.UserId == userId && k.Status == status && !k.IsDeleted);

            var keywordDtos = new List<KeywordDto>();
            foreach (var keyword in keywords.OrderByDescending(k => k.CreatedAt))
            {
                keywordDtos.Add(await MapToKeywordDtoAsync(keyword));
            }

            return keywordDtos;
        }

        public async Task<KeywordDto> CreateKeywordAsync(string userId, CreateKeywordDto dto)
        {
            // Normalize keyword text (uppercase, trim)
            var normalizedKeywordText = dto.KeywordText.Trim().ToUpperInvariant();

            // Check if keyword is available
            var isAvailable = await CheckKeywordAvailabilityAsync(normalizedKeywordText, userId);
            if (!isAvailable)
            {
                _logger.LogWarning("Keyword {KeywordText} is already in use", normalizedKeywordText);
                throw new InvalidOperationException($"Keyword '{normalizedKeywordText}' is already in use or reserved");
            }

            // Validate linked campaign if provided
            if (dto.LinkedCampaignId.HasValue)
            {
                var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                    c.Id == dto.LinkedCampaignId.Value && c.UserId == userId && !c.IsDeleted);
                if (campaign == null)
                {
                    _logger.LogWarning("Campaign {CampaignId} not found for user {UserId}", dto.LinkedCampaignId.Value, userId);
                    throw new InvalidOperationException($"Campaign with ID {dto.LinkedCampaignId.Value} not found");
                }
            }

            // Validate opt-in group if provided
            if (dto.OptInGroupId.HasValue)
            {
                var group = await _contactGroupRepository.FirstOrDefaultAsync(g =>
                    g.Id == dto.OptInGroupId.Value && g.UserId == userId && !g.IsDeleted);
                if (group == null)
                {
                    _logger.LogWarning("Contact group {GroupId} not found for user {UserId}", dto.OptInGroupId.Value, userId);
                    throw new InvalidOperationException($"Contact group with ID {dto.OptInGroupId.Value} not found");
                }
            }

            var keyword = new Keyword
            {
                UserId = userId,
                KeywordText = normalizedKeywordText,
                Description = dto.Description,
                Status = KeywordStatus.Active,
                ResponseMessage = dto.ResponseMessage,
                LinkedCampaignId = dto.LinkedCampaignId,
                OptInGroupId = dto.OptInGroupId,
                IsGloballyReserved = false
            };

            await _keywordRepository.AddAsync(keyword);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword {KeywordText} created successfully by user {UserId}", normalizedKeywordText, userId);

            var createdKeyword = await GetKeywordByIdAsync(userId, keyword.Id);
            if (createdKeyword == null)
            {
                _logger.LogError("Failed to retrieve created keyword {KeywordId}", keyword.Id);
                throw new InvalidOperationException($"Failed to retrieve created keyword with ID {keyword.Id}");
            }

            return createdKeyword;
        }

        public async Task<bool> UpdateKeywordAsync(string userId, int keywordId, UpdateKeywordDto dto)
        {
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == keywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
                return false;

            // Check if keyword is globally reserved and prevent modification
            if (keyword.IsGloballyReserved)
            {
                _logger.LogWarning("Attempted to update globally reserved keyword {KeywordId}", keywordId);
                return false;
            }

            // Normalize new keyword text
            var normalizedKeywordText = dto.KeywordText.Trim().ToUpperInvariant();

            // Check if keyword text changed and if new text is available
            if (keyword.KeywordText != normalizedKeywordText)
            {
                var isAvailable = await CheckKeywordAvailabilityAsync(normalizedKeywordText, userId);
                if (!isAvailable)
                {
                    _logger.LogWarning("Keyword {KeywordText} is already in use", normalizedKeywordText);
                    throw new InvalidOperationException($"Keyword '{normalizedKeywordText}' is already in use or reserved");
                }
            }

            // Validate linked campaign if provided
            if (dto.LinkedCampaignId.HasValue)
            {
                var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                    c.Id == dto.LinkedCampaignId.Value && c.UserId == userId && !c.IsDeleted);
                if (campaign == null)
                {
                    _logger.LogWarning("Campaign {CampaignId} not found for user {UserId}", dto.LinkedCampaignId.Value, userId);
                    throw new InvalidOperationException($"Campaign with ID {dto.LinkedCampaignId.Value} not found");
                }
            }

            // Validate opt-in group if provided
            if (dto.OptInGroupId.HasValue)
            {
                var group = await _contactGroupRepository.FirstOrDefaultAsync(g =>
                    g.Id == dto.OptInGroupId.Value && g.UserId == userId && !g.IsDeleted);
                if (group == null)
                {
                    _logger.LogWarning("Contact group {GroupId} not found for user {UserId}", dto.OptInGroupId.Value, userId);
                    throw new InvalidOperationException($"Contact group with ID {dto.OptInGroupId.Value} not found");
                }
            }

            // Update keyword properties
            keyword.KeywordText = normalizedKeywordText;
            keyword.Description = dto.Description;
            keyword.Status = dto.Status;
            keyword.ResponseMessage = dto.ResponseMessage;
            keyword.LinkedCampaignId = dto.LinkedCampaignId;
            keyword.OptInGroupId = dto.OptInGroupId;
            keyword.UpdatedAt = DateTime.UtcNow;

            _keywordRepository.Update(keyword);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword {KeywordId} updated successfully", keywordId);

            return true;
        }

        public async Task<bool> DeleteKeywordAsync(string userId, int keywordId)
        {
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == keywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
                return false;

            // Check if keyword is globally reserved and prevent deletion
            if (keyword.IsGloballyReserved)
            {
                _logger.LogWarning("Attempted to delete globally reserved keyword {KeywordId}", keywordId);
                return false;
            }

            // Soft delete
            keyword.IsDeleted = true;
            keyword.UpdatedAt = DateTime.UtcNow;

            _keywordRepository.Update(keyword);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword {KeywordId} deleted successfully", keywordId);

            return true;
        }

        public async Task<bool> CheckKeywordAvailabilityAsync(string keywordText, string userId)
        {
            var normalizedKeywordText = keywordText.Trim().ToUpperInvariant();

            // Check if keyword is globally reserved
            var globallyReserved = await _keywordRepository.AnyAsync(k =>
                k.KeywordText == normalizedKeywordText && k.IsGloballyReserved && !k.IsDeleted);

            if (globallyReserved)
                return false;

            // Check if keyword is already in use by this user
            var existingKeyword = await _keywordRepository.AnyAsync(k =>
                k.KeywordText == normalizedKeywordText && k.UserId == userId && !k.IsDeleted);

            return !existingKeyword;
        }

        public async Task<PaginatedResult<KeywordActivityDto>> GetKeywordActivitiesAsync(int keywordId, string userId, PagedRequest request)
        {
            // First verify the keyword belongs to the user
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == keywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
            {
                _logger.LogWarning("Keyword {KeywordId} not found for user {UserId}", keywordId, userId);
                throw new InvalidOperationException($"Keyword with ID {keywordId} not found");
            }

            var query = (await _keywordActivityRepository.FindAsync(ka =>
                ka.KeywordId == keywordId)).AsQueryable();

            // Apply sorting
            query = query.OrderByDescending(ka => ka.ReceivedAt);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var activities = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var activityDtos = activities.Select(a => new KeywordActivityDto
            {
                Id = a.Id,
                KeywordId = a.KeywordId,
                KeywordText = keyword.KeywordText,
                PhoneNumber = a.PhoneNumber,
                IncomingMessage = a.IncomingMessage,
                ResponseSent = a.ResponseSent,
                ReceivedAt = a.ReceivedAt
            }).ToList();

            return new PaginatedResult<KeywordActivityDto>
            {
                Items = activityDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<KeywordActivityDto> ProcessInboundKeywordAsync(string phoneNumber, string message)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("ProcessInboundKeywordAsync called with null or empty phoneNumber");
                throw new ArgumentException("Phone number is required", nameof(phoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("ProcessInboundKeywordAsync called with null or empty message");
                throw new ArgumentException("Message is required", nameof(message));
            }

            // Extract the first word as the keyword
            var keywordText = message.Trim().Split(' ')[0].ToUpperInvariant();

            // Find matching keyword
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.KeywordText == keywordText && k.Status == KeywordStatus.Active && !k.IsDeleted);

            if (keyword == null)
            {
                _logger.LogInformation("No active keyword found for text: {KeywordText}", keywordText);
                // Return DTO directly for unknown keyword (not saved to database with KeywordId = 0)
                return new KeywordActivityDto
                {
                    Id = 0,
                    KeywordId = 0,
                    KeywordText = keywordText,
                    PhoneNumber = phoneNumber,
                    IncomingMessage = message,
                    ResponseSent = null,
                    ReceivedAt = DateTime.UtcNow
                };
            }

            // Log keyword activity
            var activity = new KeywordActivity
            {
                KeywordId = keyword.Id,
                PhoneNumber = phoneNumber,
                IncomingMessage = message,
                ReceivedAt = DateTime.UtcNow
            };

            // Send response if configured
            string? responseSent = null;
            if (!string.IsNullOrWhiteSpace(keyword.ResponseMessage))
            {
                try
                {
                    await _smsProvider.SendSMSAsync(phoneNumber, keyword.ResponseMessage);
                    responseSent = keyword.ResponseMessage;
                    _logger.LogInformation("Auto-response sent for keyword {KeywordId} to {PhoneNumber}", keyword.Id, phoneNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send auto-response for keyword {KeywordId}", keyword.Id);
                }
            }

            activity.ResponseSent = responseSent;

            await _keywordActivityRepository.AddAsync(activity);
            await _unitOfWork.SaveChangesAsync();

            // Add contact to opt-in group if configured
            if (keyword.OptInGroupId.HasValue)
            {
                try
                {
                    // Check if contact exists with this phone number
                    var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                        c.PhoneNumber == phoneNumber && c.UserId == keyword.UserId && !c.IsDeleted);

                    if (contact != null)
                    {
                        // Check if contact is already in the group
                        var existingMember = await _groupMemberRepository.FirstOrDefaultAsync(gm =>
                            gm.ContactId == contact.Id && gm.ContactGroupId == keyword.OptInGroupId.Value && !gm.IsDeleted);

                        if (existingMember == null)
                        {
                            // Add contact to group
                            var newMember = new ContactGroupMember
                            {
                                ContactGroupId = keyword.OptInGroupId.Value,
                                ContactId = contact.Id
                            };
                            await _groupMemberRepository.AddAsync(newMember);
                            await _unitOfWork.SaveChangesAsync();

                            _logger.LogInformation("Contact {ContactId} added to group {GroupId} via keyword {KeywordId}",
                                contact.Id, keyword.OptInGroupId.Value, keyword.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Contact with phone number {PhoneNumber} not found for opt-in group addition", phoneNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add contact to opt-in group for keyword {KeywordId}", keyword.Id);
                }
            }

            var activityDto = _mapper.Map<KeywordActivityDto>(activity);
            activityDto.KeywordText = keyword.KeywordText;
            return activityDto;
        }

        public async Task<int> GetKeywordActivityCountAsync(int keywordId)
        {
            return await _keywordActivityRepository.CountAsync(ka => ka.KeywordId == keywordId);
        }

        public async Task<KeywordAnalyticsDto?> GetKeywordAnalyticsAsync(int keywordId, string userId)
        {
            // First verify the keyword belongs to the user
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == keywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
            {
                _logger.LogWarning("Keyword {KeywordId} not found for user {UserId}", keywordId, userId);
                return null;
            }

            // Calculate time-based metrics cutoffs
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            // Get activities - FindAsync returns IEnumerable which is streamed during iteration
            // This avoids loading all records into memory at once
            var activities = await _keywordActivityRepository.FindAsync(ka => ka.KeywordId == keywordId);
            
            // Process activities in a single pass for optimal performance
            var totalResponses = 0;
            var uniquePhoneNumbers = new HashSet<string>();
            var responsesSent = 0;
            var activitiesLast24Hours = 0;
            var activitiesLast7Days = 0;
            var activitiesLast30Days = 0;
            DateTime? firstUsedAt = null;
            DateTime? lastUsedAt = null;

            foreach (var activity in activities)
            {
                totalResponses++;
                uniquePhoneNumbers.Add(activity.PhoneNumber);
                
                if (!string.IsNullOrWhiteSpace(activity.ResponseSent))
                    responsesSent++;
                
                if (activity.ReceivedAt >= last24Hours)
                    activitiesLast24Hours++;
                if (activity.ReceivedAt >= last7Days)
                    activitiesLast7Days++;
                if (activity.ReceivedAt >= last30Days)
                    activitiesLast30Days++;
                
                if (firstUsedAt == null || activity.ReceivedAt < firstUsedAt)
                    firstUsedAt = activity.ReceivedAt;
                if (lastUsedAt == null || activity.ReceivedAt > lastUsedAt)
                    lastUsedAt = activity.ReceivedAt;
            }

            var uniqueContacts = uniquePhoneNumbers.Count;
            var repeatUsageCount = totalResponses - uniqueContacts;
            var responsesFailed = totalResponses - responsesSent;
            var responseSuccessRate = totalResponses > 0 
                ? Math.Round((decimal)responsesSent / totalResponses * 100, 2) 
                : 0;

            // Opt-in statistics - only calculate if opt-in group is configured
            int totalOptIns = 0;
            int successfulOptIns = 0;
            int failedOptIns = 0;

            if (keyword.OptInGroupId.HasValue && uniquePhoneNumbers.Count > 0)
            {
                // Get contacts matching the phone numbers efficiently
                var phoneNumbersList = uniquePhoneNumbers.ToList();
                var batchSize = 100; // Process in batches to avoid large IN clauses
                var allContactIds = new List<int>();

                for (int i = 0; i < phoneNumbersList.Count; i += batchSize)
                {
                    var batch = phoneNumbersList.Skip(i).Take(batchSize).ToList();
                    var batchContacts = await _contactRepository.FindAsync(c => 
                        batch.Contains(c.PhoneNumber) && 
                        c.UserId == userId && 
                        !c.IsDeleted);
                    
                    allContactIds.AddRange(batchContacts.Select(c => c.Id));
                }

                totalOptIns = allContactIds.Count;

                if (totalOptIns > 0)
                {
                    // Check group membership in batches
                    successfulOptIns = 0;
                    for (int i = 0; i < allContactIds.Count; i += batchSize)
                    {
                        var batch = allContactIds.Skip(i).Take(batchSize).ToList();
                        var batchMembers = await _groupMemberRepository.FindAsync(gm =>
                            gm.ContactGroupId == keyword.OptInGroupId.Value &&
                            batch.Contains(gm.ContactId) &&
                            !gm.IsDeleted);
                        
                        successfulOptIns += batchMembers.Count();
                    }
                    
                    failedOptIns = totalOptIns - successfulOptIns;
                }
            }

            var optInConversionRate = totalResponses > 0 
                ? Math.Round((decimal)successfulOptIns / totalResponses * 100, 2) 
                : 0;

            // Campaign statistics
            var campaignRelatedActivities = keyword.LinkedCampaignId.HasValue ? totalResponses : 0;
            string? linkedCampaignName = null;

            if (keyword.LinkedCampaignId.HasValue)
            {
                var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                    c.Id == keyword.LinkedCampaignId.Value && !c.IsDeleted);
                linkedCampaignName = campaign?.Name;
            }

            return new KeywordAnalyticsDto
            {
                KeywordId = keyword.Id,
                KeywordText = keyword.KeywordText,
                TotalResponses = totalResponses,
                UniqueContacts = uniqueContacts,
                RepeatUsageCount = repeatUsageCount,
                TotalOptIns = totalOptIns,
                SuccessfulOptIns = successfulOptIns,
                FailedOptIns = failedOptIns,
                OptInConversionRate = optInConversionRate,
                ResponsesSent = responsesSent,
                ResponsesFailed = responsesFailed,
                ResponseSuccessRate = responseSuccessRate,
                LinkedCampaignId = keyword.LinkedCampaignId,
                LinkedCampaignName = linkedCampaignName,
                CampaignRelatedActivities = campaignRelatedActivities,
                FirstUsedAt = firstUsedAt,
                LastUsedAt = lastUsedAt,
                ActivitiesLast24Hours = activitiesLast24Hours,
                ActivitiesLast7Days = activitiesLast7Days,
                ActivitiesLast30Days = activitiesLast30Days
            };
        }

        private async Task<KeywordDto> MapToKeywordDtoAsync(Keyword keyword)
        {
            var dto = _mapper.Map<KeywordDto>(keyword);

            // Load linked campaign name
            if (keyword.LinkedCampaignId.HasValue)
            {
                var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                    c.Id == keyword.LinkedCampaignId.Value && !c.IsDeleted);
                dto.LinkedCampaignName = campaign?.Name;
            }

            // Load opt-in group name
            if (keyword.OptInGroupId.HasValue)
            {
                var group = await _contactGroupRepository.FirstOrDefaultAsync(g =>
                    g.Id == keyword.OptInGroupId.Value && !g.IsDeleted);
                dto.OptInGroupName = group?.Name;
            }

            // Load activity count
            dto.ActivityCount = await GetKeywordActivityCountAsync(keyword.Id);

            return dto;
        }

        // Keyword Reservation Methods (12.4.1)
        public async Task<KeywordReservationDto> CreateReservationAsync(string userId, CreateKeywordReservationDto dto)
        {
            var normalizedKeywordText = dto.KeywordText.Trim().ToUpperInvariant();

            // Check if keyword is already reserved or in use
            var existingKeyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.KeywordText == normalizedKeywordText && !k.IsDeleted);
            
            if (existingKeyword != null)
            {
                // Check for existing reservation or create conflict
                var existingReservation = await _keywordReservationRepository.FirstOrDefaultAsync(r =>
                    r.KeywordText == normalizedKeywordText && 
                    r.Status != ReservationStatus.Rejected && 
                    r.Status != ReservationStatus.Expired &&
                    !r.IsDeleted);

                if (existingReservation != null)
                {
                    _logger.LogWarning("Keyword {KeywordText} is already reserved", normalizedKeywordText);
                    throw new InvalidOperationException($"Keyword '{normalizedKeywordText}' is already reserved");
                }

                // Create conflict record
                var conflict = new KeywordConflict
                {
                    KeywordText = normalizedKeywordText,
                    RequestingUserId = userId,
                    ExistingUserId = existingKeyword.UserId,
                    Status = ReservationStatus.Pending
                };
                await _keywordConflictRepository.AddAsync(conflict);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Keyword conflict created for {KeywordText}", normalizedKeywordText);
                throw new InvalidOperationException($"Keyword '{normalizedKeywordText}' is already in use. A conflict has been created for resolution.");
            }

            var reservation = new KeywordReservation
            {
                KeywordText = normalizedKeywordText,
                RequestedByUserId = userId,
                Status = ReservationStatus.Pending,
                Purpose = dto.Purpose,
                ExpiresAt = dto.ExpiresAt,
                Priority = dto.Priority
            };

            await _keywordReservationRepository.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword reservation created for {KeywordText} by user {UserId}", normalizedKeywordText, userId);

            return await GetReservationByIdAsync(reservation.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created reservation");
        }

        public async Task<KeywordReservationDto?> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _keywordReservationRepository.FirstOrDefaultAsync(r =>
                r.Id == reservationId && !r.IsDeleted);

            if (reservation == null)
                return null;

            var dto = _mapper.Map<KeywordReservationDto>(reservation);

            // Load user names
            var requestedBy = await _userRepository.FirstOrDefaultAsync(u => u.Id == reservation.RequestedByUserId);
            dto.RequestedByUserName = requestedBy?.UserName;

            if (reservation.ApprovedByUserId != null)
            {
                var approvedBy = await _userRepository.FirstOrDefaultAsync(u => u.Id == reservation.ApprovedByUserId);
                dto.ApprovedByUserName = approvedBy?.UserName;
            }

            return dto;
        }

        public async Task<PaginatedResult<KeywordReservationDto>> GetReservationsAsync(PagedRequest request)
        {
            var query = (await _keywordReservationRepository.FindAsync(r => !r.IsDeleted)).AsQueryable();

            // Apply sorting
            query = query.OrderByDescending(r => r.CreatedAt);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var reservations = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<KeywordReservationDto>();
            foreach (var reservation in reservations)
            {
                var dto = await GetReservationByIdAsync(reservation.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return new PaginatedResult<KeywordReservationDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<bool> UpdateReservationAsync(string userId, int reservationId, UpdateKeywordReservationDto dto)
        {
            var reservation = await _keywordReservationRepository.FirstOrDefaultAsync(r =>
                r.Id == reservationId && r.RequestedByUserId == userId && !r.IsDeleted);

            if (reservation == null)
                return false;

            reservation.Status = dto.Status;
            reservation.ExpiresAt = dto.ExpiresAt;
            reservation.RejectionReason = dto.RejectionReason;
            reservation.Priority = dto.Priority;
            reservation.UpdatedAt = DateTime.UtcNow;

            _keywordReservationRepository.Update(reservation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword reservation {ReservationId} updated", reservationId);
            return true;
        }

        public async Task<bool> ApproveReservationAsync(string approverUserId, int reservationId)
        {
            var reservation = await _keywordReservationRepository.FirstOrDefaultAsync(r =>
                r.Id == reservationId && !r.IsDeleted);

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return false;

            reservation.Status = ReservationStatus.Approved;
            reservation.ApprovedByUserId = approverUserId;
            reservation.ApprovedAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            _keywordReservationRepository.Update(reservation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword reservation {ReservationId} approved by {UserId}", reservationId, approverUserId);
            return true;
        }

        public async Task<bool> RejectReservationAsync(string approverUserId, int reservationId, string reason)
        {
            var reservation = await _keywordReservationRepository.FirstOrDefaultAsync(r =>
                r.Id == reservationId && !r.IsDeleted);

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return false;

            reservation.Status = ReservationStatus.Rejected;
            reservation.ApprovedByUserId = approverUserId;
            reservation.RejectionReason = reason;
            reservation.UpdatedAt = DateTime.UtcNow;

            _keywordReservationRepository.Update(reservation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword reservation {ReservationId} rejected by {UserId}", reservationId, approverUserId);
            return true;
        }

        // Keyword Assignment Methods (12.4.2)
        public async Task<KeywordAssignmentDto> AssignKeywordToCampaignAsync(string userId, CreateKeywordAssignmentDto dto)
        {
            // Verify keyword exists and belongs to user
            var keyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.Id == dto.KeywordId && k.UserId == userId && !k.IsDeleted);

            if (keyword == null)
            {
                _logger.LogWarning("Keyword {KeywordId} not found for user {UserId}", dto.KeywordId, userId);
                throw new InvalidOperationException($"Keyword with ID {dto.KeywordId} not found");
            }

            // Verify campaign exists and belongs to user
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == dto.CampaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
            {
                _logger.LogWarning("Campaign {CampaignId} not found for user {UserId}", dto.CampaignId, userId);
                throw new InvalidOperationException($"Campaign with ID {dto.CampaignId} not found");
            }

            // Check if keyword is already assigned to this campaign
            var existingAssignment = await _keywordAssignmentRepository.FirstOrDefaultAsync(a =>
                a.KeywordId == dto.KeywordId && a.CampaignId == dto.CampaignId && a.IsActive && !a.IsDeleted);

            if (existingAssignment != null)
            {
                _logger.LogWarning("Keyword {KeywordId} is already assigned to campaign {CampaignId}", dto.KeywordId, dto.CampaignId);
                throw new InvalidOperationException("Keyword is already assigned to this campaign");
            }

            var assignment = new KeywordAssignment
            {
                KeywordId = dto.KeywordId,
                CampaignId = dto.CampaignId,
                AssignedByUserId = userId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                Notes = dto.Notes
            };

            await _keywordAssignmentRepository.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword {KeywordId} assigned to campaign {CampaignId}", dto.KeywordId, dto.CampaignId);

            return await GetAssignmentByIdAsync(assignment.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created assignment");
        }

        public async Task<KeywordAssignmentDto?> GetAssignmentByIdAsync(int assignmentId)
        {
            var assignment = await _keywordAssignmentRepository.FirstOrDefaultAsync(a =>
                a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null)
                return null;

            var dto = _mapper.Map<KeywordAssignmentDto>(assignment);

            // Load related data
            var keyword = await _keywordRepository.GetByIdAsync(assignment.KeywordId);
            dto.KeywordText = keyword?.KeywordText ?? "";

            var campaign = await _campaignRepository.GetByIdAsync(assignment.CampaignId);
            dto.CampaignName = campaign?.Name;

            var assignedBy = await _userRepository.FirstOrDefaultAsync(u => u.Id == assignment.AssignedByUserId);
            dto.AssignedByUserName = assignedBy?.UserName;

            return dto;
        }

        public async Task<PaginatedResult<KeywordAssignmentDto>> GetAssignmentsAsync(PagedRequest request)
        {
            var query = (await _keywordAssignmentRepository.FindAsync(a => !a.IsDeleted)).AsQueryable();

            // Apply sorting
            query = query.OrderByDescending(a => a.AssignedAt);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var assignments = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<KeywordAssignmentDto>();
            foreach (var assignment in assignments)
            {
                var dto = await GetAssignmentByIdAsync(assignment.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return new PaginatedResult<KeywordAssignmentDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<List<KeywordAssignmentDto>> GetAssignmentsByCampaignAsync(int campaignId)
        {
            var assignments = await _keywordAssignmentRepository.FindAsync(a =>
                a.CampaignId == campaignId && a.IsActive && !a.IsDeleted);

            var dtos = new List<KeywordAssignmentDto>();
            foreach (var assignment in assignments)
            {
                var dto = await GetAssignmentByIdAsync(assignment.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<bool> UnassignKeywordAsync(string userId, int assignmentId)
        {
            var assignment = await _keywordAssignmentRepository.FirstOrDefaultAsync(a =>
                a.Id == assignmentId && a.AssignedByUserId == userId && !a.IsDeleted);

            if (assignment == null)
                return false;

            assignment.IsActive = false;
            assignment.UnassignedAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            _keywordAssignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword assignment {AssignmentId} unassigned", assignmentId);
            return true;
        }

        // Keyword Conflict Resolution Methods (12.4.3)
        public async Task<KeywordConflictDto?> CheckForConflictAsync(string keywordText, string requestingUserId)
        {
            var normalizedKeywordText = keywordText.Trim().ToUpperInvariant();

            // Check if keyword is in use by another user
            var existingKeyword = await _keywordRepository.FirstOrDefaultAsync(k =>
                k.KeywordText == normalizedKeywordText && k.UserId != requestingUserId && !k.IsDeleted);

            if (existingKeyword == null)
                return null;

            // Check if conflict already exists
            var existingConflict = await _keywordConflictRepository.FirstOrDefaultAsync(c =>
                c.KeywordText == normalizedKeywordText &&
                c.RequestingUserId == requestingUserId &&
                c.Status == ReservationStatus.Pending &&
                !c.IsDeleted);

            if (existingConflict != null)
            {
                var dto = _mapper.Map<KeywordConflictDto>(existingConflict);
                
                var requestingUser = await _userRepository.FirstOrDefaultAsync(u => u.Id == existingConflict.RequestingUserId);
                dto.RequestingUserName = requestingUser?.UserName;

                var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Id == existingConflict.ExistingUserId);
                dto.ExistingUserName = existingUser?.UserName;

                return dto;
            }

            return null;
        }

        public async Task<PaginatedResult<KeywordConflictDto>> GetConflictsAsync(PagedRequest request)
        {
            var query = (await _keywordConflictRepository.FindAsync(c => !c.IsDeleted)).AsQueryable();

            // Apply sorting - pending conflicts first
            query = query.OrderBy(c => c.Status == ReservationStatus.Pending ? 0 : 1)
                         .ThenByDescending(c => c.CreatedAt);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var conflicts = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<KeywordConflictDto>();
            foreach (var conflict in conflicts)
            {
                var dto = _mapper.Map<KeywordConflictDto>(conflict);
                
                var requestingUser = await _userRepository.FirstOrDefaultAsync(u => u.Id == conflict.RequestingUserId);
                dto.RequestingUserName = requestingUser?.UserName;

                var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Id == conflict.ExistingUserId);
                dto.ExistingUserName = existingUser?.UserName;

                if (conflict.ResolvedByUserId != null)
                {
                    var resolvedBy = await _userRepository.FirstOrDefaultAsync(u => u.Id == conflict.ResolvedByUserId);
                    dto.ResolvedByUserName = resolvedBy?.UserName;
                }

                dtos.Add(dto);
            }

            return new PaginatedResult<KeywordConflictDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<bool> ResolveConflictAsync(string resolverUserId, int conflictId, ResolveKeywordConflictDto dto)
        {
            var conflict = await _keywordConflictRepository.FirstOrDefaultAsync(c =>
                c.Id == conflictId && !c.IsDeleted);

            if (conflict == null || conflict.Status != ReservationStatus.Pending)
                return false;

            conflict.Status = dto.Status;
            conflict.Resolution = dto.Resolution;
            conflict.ResolvedByUserId = resolverUserId;
            conflict.ResolvedAt = DateTime.UtcNow;
            conflict.UpdatedAt = DateTime.UtcNow;

            _keywordConflictRepository.Update(conflict);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Keyword conflict {ConflictId} resolved by {UserId} with status {Status}", 
                conflictId, resolverUserId, dto.Status);
            return true;
        }
    }
}
