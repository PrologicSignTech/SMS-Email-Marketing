using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Template;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace MarketingPlatform.Application.Services
{
    public class TemplateService : ITemplateService
    {
        // SMS/MMS character limits
        private const int SmsGsm7SingleSegment = 160;
        private const int SmsGsm7ConcatenatedSegment = 153;
        private const int SmsUnicodeSingleSegment = 70;
        private const int SmsUnicodeConcatenatedSegment = 67;
        
        // Email limits
        private const int EmailSubjectRecommendedMax = 60;
        
        // Unicode special characters not in GSM-7 basic set
        private static readonly char[] UnicodeSpecialChars = { '€', '[', ']', '{', '}', '\\', '^', '~', '|' };

        private readonly IRepository<MessageTemplate> _templateRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<CampaignContent> _campaignContentRepository;
        private readonly IRepository<CampaignMessage> _campaignMessageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TemplateService> _logger;

        public TemplateService(
            IRepository<MessageTemplate> templateRepository,
            IRepository<Contact> contactRepository,
            IRepository<CampaignContent> campaignContentRepository,
            IRepository<CampaignMessage> campaignMessageRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TemplateService> logger)
        {
            _templateRepository = templateRepository;
            _contactRepository = contactRepository;
            _campaignContentRepository = campaignContentRepository;
            _campaignMessageRepository = campaignMessageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TemplateDto?> GetTemplateByIdAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return null;

            return MapToDto(template);
        }

        public async Task<PaginatedResult<TemplateDto>> GetTemplatesAsync(string userId, PagedRequest request)
        {
            var query = (await _templateRepository.FindAsync(t =>
                t.UserId == userId && !t.IsDeleted)).AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(t =>
                    t.Name.Contains(request.SearchTerm) ||
                    (t.Description != null && t.Description.Contains(request.SearchTerm)));
            }

            // Sort
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "usagecount" => request.SortDescending ? query.OrderByDescending(t => t.UsageCount) : query.OrderBy(t => t.UsageCount),
                _ => query.OrderByDescending(t => t.IsDefault).ThenByDescending(t => t.CreatedAt)
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => MapToDto(t))
                .ToList();

            return new PaginatedResult<TemplateDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<List<TemplateDto>> GetTemplatesByChannelAsync(string userId, ChannelType channel)
        {
            var templates = await _templateRepository.FindAsync(t =>
                t.UserId == userId && t.Channel == channel && !t.IsDeleted);

            return templates
                .OrderByDescending(t => t.IsDefault)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => MapToDto(t))
                .ToList();
        }

        public async Task<List<TemplateDto>> GetTemplatesByCategoryAsync(string userId, TemplateCategory category)
        {
            var templates = await _templateRepository.FindAsync(t =>
                t.UserId == userId && t.Category == category && !t.IsDeleted);

            return templates
                .OrderByDescending(t => t.IsDefault)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => MapToDto(t))
                .ToList();
        }

        public async Task<TemplateDto> CreateTemplateAsync(string userId, CreateTemplateDto dto)
        {
            var template = _mapper.Map<MessageTemplate>(dto);
            template.UserId = userId;

            // Auto-extract variables from content
            var extractedVariables = await ExtractVariablesFromContentAsync(dto.MessageBody);
            if (!string.IsNullOrWhiteSpace(dto.Subject))
            {
                extractedVariables.AddRange(await ExtractVariablesFromContentAsync(dto.Subject));
            }
            if (!string.IsNullOrWhiteSpace(dto.HTMLContent))
            {
                extractedVariables.AddRange(await ExtractVariablesFromContentAsync(dto.HTMLContent));
            }

            // Merge with provided variables
            var variableList = dto.Variables.ToList();
            foreach (var varName in extractedVariables.Distinct())
            {
                if (!variableList.Any(v => v.Name.Equals(varName, StringComparison.OrdinalIgnoreCase)))
                {
                    variableList.Add(new TemplateVariableDto
                    {
                        Name = varName,
                        IsRequired = false
                    });
                }
            }

            template.TemplateVariables = JsonConvert.SerializeObject(variableList);
            template.DefaultMediaUrls = dto.DefaultMediaUrls != null && dto.DefaultMediaUrls.Any()
                ? JsonConvert.SerializeObject(dto.DefaultMediaUrls)
                : null;

            await _templateRepository.AddAsync(template);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(template);
        }

        public async Task<bool> UpdateTemplateAsync(string userId, int templateId, UpdateTemplateDto dto)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.Subject = dto.Subject;
            template.MessageBody = dto.MessageBody;
            template.HTMLContent = dto.HTMLContent;
            template.UpdatedAt = DateTime.UtcNow;

            if (dto.DefaultMediaUrls != null)
            {
                template.DefaultMediaUrls = dto.DefaultMediaUrls.Any()
                    ? JsonConvert.SerializeObject(dto.DefaultMediaUrls)
                    : null;
            }

            if (dto.Variables != null)
            {
                template.TemplateVariables = JsonConvert.SerializeObject(dto.Variables);
            }
            else
            {
                // Auto-extract variables
                var extractedVariables = await ExtractVariablesFromContentAsync(dto.MessageBody);
                if (!string.IsNullOrWhiteSpace(dto.Subject))
                {
                    extractedVariables.AddRange(await ExtractVariablesFromContentAsync(dto.Subject));
                }
                if (!string.IsNullOrWhiteSpace(dto.HTMLContent))
                {
                    extractedVariables.AddRange(await ExtractVariablesFromContentAsync(dto.HTMLContent));
                }

                var variableList = extractedVariables.Distinct().Select(v => new TemplateVariableDto
                {
                    Name = v,
                    IsRequired = false
                }).ToList();

                template.TemplateVariables = JsonConvert.SerializeObject(variableList);
            }

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTemplateAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            // Cannot delete if in use
            if (template.UsageCount > 0)
            {
                _logger.LogWarning($"Cannot delete template {templateId} - currently in use");
                return false;
            }

            template.IsDeleted = true;
            template.UpdatedAt = DateTime.UtcNow;

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DuplicateTemplateAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            var duplicate = new MessageTemplate
            {
                UserId = userId,
                Name = $"{template.Name} (Copy)",
                Description = template.Description,
                Channel = template.Channel,
                Category = template.Category,
                Subject = template.Subject,
                MessageBody = template.MessageBody,
                HTMLContent = template.HTMLContent,
                DefaultMediaUrls = template.DefaultMediaUrls,
                TemplateVariables = template.TemplateVariables,
                IsActive = template.IsActive,
                IsDefault = false, // Copies are never default
                UsageCount = 0
            };

            await _templateRepository.AddAsync(duplicate);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultTemplateAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            // Clear other defaults for the same channel + category
            var otherDefaults = await _templateRepository.FindAsync(t =>
                t.UserId == userId &&
                t.Channel == template.Channel &&
                t.Category == template.Category &&
                t.IsDefault &&
                t.Id != templateId &&
                !t.IsDeleted);

            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
                other.UpdatedAt = DateTime.UtcNow;
                _templateRepository.Update(other);
            }

            template.IsDefault = true;
            template.UpdatedAt = DateTime.UtcNow;
            _templateRepository.Update(template);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateTemplateAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            template.IsActive = true;
            template.UpdatedAt = DateTime.UtcNow;

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateTemplateAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                return false;

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<TemplatePreviewDto> PreviewTemplateAsync(string userId, TemplatePreviewRequestDto request)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == request.TemplateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                throw new KeyNotFoundException("Template not found");

            var variables = new Dictionary<string, string>(request.VariableValues, StringComparer.OrdinalIgnoreCase);

            // Load contact data if ContactId provided
            if (request.ContactId.HasValue)
            {
                var contactVars = await LoadContactVariablesAsync(request.ContactId.Value);
                foreach (var kvp in contactVars)
                {
                    if (!variables.ContainsKey(kvp.Key))
                    {
                        variables[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Render content
            var renderedSubject = !string.IsNullOrWhiteSpace(template.Subject)
                ? RenderContent(template.Subject, variables)
                : null;
            var renderedBody = RenderContent(template.MessageBody, variables);
            
            var preview = new TemplatePreviewDto
            {
                Subject = renderedSubject,
                MessageBody = renderedBody,
                HTMLContent = !string.IsNullOrWhiteSpace(template.HTMLContent)
                    ? RenderContent(template.HTMLContent, variables)
                    : null,
                MediaUrls = !string.IsNullOrWhiteSpace(template.DefaultMediaUrls)
                    ? JsonConvert.DeserializeObject<List<string>>(template.DefaultMediaUrls)
                    : null,
                SubjectCharacterCount = renderedSubject != null ? CalculateCharacterCount(renderedSubject, ChannelType.Email, true) : null,
                MessageBodyCharacterCount = CalculateCharacterCount(renderedBody, template.Channel, false)
            };

            // Find missing variables
            preview.MissingVariables = FindMissingVariables(template.MessageBody, variables);
            if (!string.IsNullOrWhiteSpace(template.Subject))
            {
                preview.MissingVariables.AddRange(FindMissingVariables(template.Subject, variables));
            }
            if (!string.IsNullOrWhiteSpace(template.HTMLContent))
            {
                preview.MissingVariables.AddRange(FindMissingVariables(template.HTMLContent, variables));
            }
            preview.MissingVariables = preview.MissingVariables.Distinct().ToList();

            return preview;
        }

        public async Task<string> RenderTemplateAsync(int templateId, Dictionary<string, string> variables)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null || template.IsDeleted)
                throw new KeyNotFoundException("Template not found");

            return RenderContent(template.MessageBody, variables);
        }

        public Task<List<string>> ExtractVariablesFromContentAsync(string content)
        {
            return Task.FromResult(ExtractVariablesFromContent(content));
        }

        public async Task<TemplateUsageStatsDto> GetTemplateUsageStatsAsync(string userId, int templateId)
        {
            var template = await _templateRepository.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.UserId == userId && !t.IsDeleted);

            if (template == null)
                throw new KeyNotFoundException("Template not found");

            // Get campaign contents using this template
            var campaignContents = await _campaignContentRepository.FindAsync(cc =>
                cc.MessageTemplateId == templateId && !cc.IsDeleted);

            var totalCampaigns = campaignContents.Select(cc => cc.CampaignId).Distinct().Count();

            // Get total messages and success rate
            var campaignIds = campaignContents.Select(cc => cc.CampaignId).Distinct().ToList();
            var messages = await _campaignMessageRepository.FindAsync(cm =>
                campaignIds.Contains(cm.CampaignId) && !cm.IsDeleted);

            var totalMessages = messages.Count();
            var successfulMessages = messages.Count(m =>
                m.Status == MessageStatus.Sent || m.Status == MessageStatus.Delivered);

            var successRate = totalMessages > 0
                ? Math.Round((decimal)successfulMessages / totalMessages * 100, 2)
                : 0m;

            return new TemplateUsageStatsDto
            {
                TemplateId = templateId,
                TemplateName = template.Name,
                TotalCampaigns = totalCampaigns,
                TotalMessages = totalMessages,
                LastUsedAt = template.LastUsedAt,
                SuccessRate = successRate
            };
        }

        public Task<CharacterCountDto> CalculateCharacterCountAsync(string content, ChannelType channel, bool isSubject)
        {
            return Task.FromResult(CalculateCharacterCount(content, channel, isSubject));
        }

        // Private helper methods
        private TemplateDto MapToDto(MessageTemplate template)
        {
            var dto = _mapper.Map<TemplateDto>(template);

            // Deserialize JSON fields
            if (!string.IsNullOrWhiteSpace(template.TemplateVariables))
            {
                dto.Variables = JsonConvert.DeserializeObject<List<TemplateVariableDto>>(template.TemplateVariables)
                    ?? new List<TemplateVariableDto>();
            }

            if (!string.IsNullOrWhiteSpace(template.DefaultMediaUrls))
            {
                dto.DefaultMediaUrls = JsonConvert.DeserializeObject<List<string>>(template.DefaultMediaUrls);
            }

            return dto;
        }

        private string RenderContent(string content, Dictionary<string, string> variables)
        {
            var result = content;

            foreach (var kvp in variables)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                // Use simple string replacement with case-insensitive comparison
                // More efficient than Regex for simple placeholder replacement
                var index = 0;
                while ((index = result.IndexOf(placeholder, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    result = result.Remove(index, placeholder.Length).Insert(index, kvp.Value);
                    index += kvp.Value.Length;
                }
            }

            return result;
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

            var regex = new Regex(@"\{\{([^}]+)\}\}");
            var matches = regex.Matches(content);

            return matches
                .Select(m => m.Groups[1].Value.Trim())
                .Distinct()
                .ToList();
        }

        private async Task<Dictionary<string, string>> LoadContactVariablesAsync(int contactId)
        {
            var contact = await _contactRepository.GetByIdAsync(contactId);
            if (contact == null || contact.IsDeleted)
                return new Dictionary<string, string>();

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FirstName", contact.FirstName ?? "" },
                { "LastName", contact.LastName ?? "" },
                { "Email", contact.Email ?? "" },
                { "Phone", contact.PhoneNumber ?? "" },
                { "PhoneNumber", contact.PhoneNumber ?? "" }
            };
        }

        private CharacterCountDto CalculateCharacterCount(string content, ChannelType channel, bool isSubject)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new CharacterCountDto
                {
                    CharacterCount = 0,
                    ContainsUnicode = false,
                    SmsSegments = channel == ChannelType.SMS || channel == ChannelType.MMS ? 0 : null,
                    RecommendedMaxLength = GetRecommendedMaxLength(channel, isSubject, false),
                    ExceedsRecommendedLength = false
                };
            }

            var charCount = content.Length;
            var containsUnicode = ContainsUnicodeCharacters(content);

            int? smsSegments = null;
            int? recommendedMax = GetRecommendedMaxLength(channel, isSubject, containsUnicode);

            // Calculate SMS segments for SMS/MMS channels
            if (channel == ChannelType.SMS || channel == ChannelType.MMS)
            {
                // Standard GSM-7 encoding: 160 chars per segment (concatenated: 153 chars per segment)
                // Unicode UCS-2 encoding: 70 chars per segment (concatenated: 67 chars per segment)
                if (containsUnicode)
                {
                    smsSegments = charCount <= SmsUnicodeSingleSegment 
                        ? 1 
                        : (int)Math.Ceiling((double)charCount / SmsUnicodeConcatenatedSegment);
                }
                else
                {
                    smsSegments = charCount <= SmsGsm7SingleSegment 
                        ? 1 
                        : (int)Math.Ceiling((double)charCount / SmsGsm7ConcatenatedSegment);
                }
            }

            return new CharacterCountDto
            {
                CharacterCount = charCount,
                ContainsUnicode = containsUnicode,
                SmsSegments = smsSegments,
                RecommendedMaxLength = recommendedMax,
                ExceedsRecommendedLength = recommendedMax.HasValue && charCount > recommendedMax.Value
            };
        }

        private bool ContainsUnicodeCharacters(string text)
        {
            // Check if string contains characters outside the GSM-7 character set
            // GSM-7 includes basic ASCII and some extended characters
            foreach (char c in text)
            {
                // Characters above ASCII 127 or certain special chars indicate Unicode
                if (c > 127 || UnicodeSpecialChars.Contains(c))
                {
                    return true;
                }
            }
            return false;
        }

        private int? GetRecommendedMaxLength(ChannelType channel, bool isSubject, bool isUnicode)
        {
            if (isSubject && channel == ChannelType.Email)
            {
                return EmailSubjectRecommendedMax;
            }

            return channel switch
            {
                ChannelType.SMS => isUnicode ? SmsUnicodeSingleSegment : SmsGsm7SingleSegment,
                ChannelType.MMS => isUnicode ? SmsUnicodeSingleSegment : SmsGsm7SingleSegment,
                ChannelType.Email => null, // No strict limit for email body
                _ => null
            };
        }
    }
}
