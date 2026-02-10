using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionRule;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class SuppressionRuleService : ISuppressionRuleService
    {
        private readonly IRepository<SuppressionRule> _ruleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SuppressionRuleService> _logger;

        public SuppressionRuleService(
            IRepository<SuppressionRule> ruleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SuppressionRuleService> logger)
        {
            _ruleRepository = ruleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResult<SuppressionRuleDto>> GetAllAsync(string userId, PagedRequest request)
        {
            var query = (await _ruleRepository.FindAsync(r =>
                r.UserId == userId && !r.IsDeleted)).AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(searchLower));
            }

            var totalCount = query.Count();
            query = query.OrderBy(r => r.Priority).ThenByDescending(r => r.CreatedAt);

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<SuppressionRuleDto>>(items);

            return new PaginatedResult<SuppressionRuleDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<SuppressionRuleDto?> GetByIdAsync(string userId, int id)
        {
            var rule = await _ruleRepository.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId && !r.IsDeleted);
            return rule == null ? null : _mapper.Map<SuppressionRuleDto>(rule);
        }

        public async Task<List<SuppressionRuleDto>> GetActiveRulesAsync(string userId)
        {
            var rules = await _ruleRepository.FindAsync(r =>
                r.UserId == userId && r.IsActive && !r.IsDeleted);
            return _mapper.Map<List<SuppressionRuleDto>>(rules.OrderBy(r => r.Priority).ToList());
        }

        public async Task<SuppressionRuleDto> CreateAsync(string userId, CreateSuppressionRuleDto dto)
        {
            var rule = _mapper.Map<SuppressionRule>(dto);
            rule.UserId = userId;
            rule.IsSystemRule = false;

            await _ruleRepository.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SuppressionRuleDto>(rule);
        }

        public async Task<SuppressionRuleDto> UpdateAsync(string userId, int id, UpdateSuppressionRuleDto dto)
        {
            var rule = await _ruleRepository.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId && !r.IsDeleted);
            if (rule == null)
                throw new InvalidOperationException("Suppression rule not found.");

            if (dto.Name != null) rule.Name = dto.Name;
            if (dto.Description != null) rule.Description = dto.Description;
            if (dto.Scope.HasValue) rule.Scope = dto.Scope.Value;
            if (dto.Channel.HasValue) rule.Channel = dto.Channel.Value;
            if (dto.SuppressionType.HasValue) rule.SuppressionType = dto.SuppressionType.Value;
            if (dto.IsActive.HasValue) rule.IsActive = dto.IsActive.Value;
            if (dto.Priority.HasValue) rule.Priority = dto.Priority.Value;
            if (dto.AutoReason != null) rule.AutoReason = dto.AutoReason;
            rule.UpdatedAt = DateTime.UtcNow;

            _ruleRepository.Update(rule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SuppressionRuleDto>(rule);
        }

        public async Task<bool> ToggleAsync(string userId, int id)
        {
            var rule = await _ruleRepository.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId && !r.IsDeleted);
            if (rule == null) return false;

            rule.IsActive = !rule.IsActive;
            rule.UpdatedAt = DateTime.UtcNow;

            _ruleRepository.Update(rule);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, int id)
        {
            var rule = await _ruleRepository.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId && !r.IsDeleted);
            if (rule == null) return false;

            if (rule.IsSystemRule)
                throw new InvalidOperationException("System rules cannot be deleted. You can only disable them.");

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;

            _ruleRepository.Update(rule);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task SeedDefaultRulesAsync(string userId)
        {
            var existingRules = await _ruleRepository.FindAsync(r =>
                r.UserId == userId && r.IsSystemRule && !r.IsDeleted);
            if (existingRules.Any()) return;

            var defaultRules = new List<SuppressionRule>
            {
                new() { UserId = userId, Name = "Unsubscribe Link Click", Description = "Automatically suppress when user clicks unsubscribe link in email", Trigger = SuppressionTrigger.Unsubscribe, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.Email, SuppressionType = SuppressionType.OptOut, IsActive = true, IsSystemRule = true, Priority = 1, AutoReason = "User clicked unsubscribe link" },
                new() { UserId = userId, Name = "Email Hard Bounce", Description = "Automatically suppress email on hard bounce (invalid address)", Trigger = SuppressionTrigger.HardBounce, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.Email, SuppressionType = SuppressionType.Bounce, IsActive = true, IsSystemRule = true, Priority = 2, AutoReason = "Email hard bounced - invalid address" },
                new() { UserId = userId, Name = "Spam Complaint", Description = "Automatically suppress when spam complaint is received", Trigger = SuppressionTrigger.SpamComplaint, Scope = SuppressionScope.Global, Channel = SuppressionChannel.All, SuppressionType = SuppressionType.Complaint, IsActive = true, IsSystemRule = true, Priority = 3, AutoReason = "Spam complaint received" },
                new() { UserId = userId, Name = "SMS Opt-Out (STOP)", Description = "Automatically suppress when user sends STOP keyword", Trigger = SuppressionTrigger.SmsOptOut, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.SMS, SuppressionType = SuppressionType.OptOut, IsActive = true, IsSystemRule = true, Priority = 4, AutoReason = "User sent STOP keyword" },
                new() { UserId = userId, Name = "WhatsApp Opt-Out", Description = "Automatically suppress when user opts out of WhatsApp messages", Trigger = SuppressionTrigger.WhatsAppOptOut, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.WhatsApp, SuppressionType = SuppressionType.OptOut, IsActive = true, IsSystemRule = true, Priority = 5, AutoReason = "WhatsApp opt-out" },
                new() { UserId = userId, Name = "Invalid Email Detected", Description = "Automatically suppress invalid email addresses", Trigger = SuppressionTrigger.InvalidEmail, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.Email, SuppressionType = SuppressionType.Bounce, IsActive = true, IsSystemRule = true, Priority = 6, AutoReason = "Invalid email address detected" },
                new() { UserId = userId, Name = "Invalid Phone Detected", Description = "Automatically suppress invalid phone numbers", Trigger = SuppressionTrigger.InvalidPhone, Scope = SuppressionScope.ChannelSpecific, Channel = SuppressionChannel.SMS, SuppressionType = SuppressionType.Bounce, IsActive = true, IsSystemRule = true, Priority = 7, AutoReason = "Invalid phone number detected" }
            };

            foreach (var rule in defaultRules)
            {
                await _ruleRepository.AddAsync(rule);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
