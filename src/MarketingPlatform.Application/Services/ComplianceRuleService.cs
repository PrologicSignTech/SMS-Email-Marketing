using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class ComplianceRuleService : IComplianceRuleService
    {
        private readonly IRepository<ComplianceRule> _complianceRuleRepository;
        private readonly IRepository<ComplianceRuleAudit> _auditRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplianceRuleService> _logger;

        public ComplianceRuleService(
            IRepository<ComplianceRule> complianceRuleRepository,
            IRepository<ComplianceRuleAudit> auditRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ComplianceRuleService> logger)
        {
            _complianceRuleRepository = complianceRuleRepository;
            _auditRepository = auditRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ComplianceRuleDto?> GetComplianceRuleByIdAsync(int id)
        {
            var rule = await _complianceRuleRepository.GetByIdAsync(id);
            return rule != null ? _mapper.Map<ComplianceRuleDto>(rule) : null;
        }

        public async Task<PaginatedResult<ComplianceRuleDto>> GetComplianceRulesAsync(PagedRequest request)
        {
            var query = _complianceRuleRepository.GetQueryable().Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(r =>
                    r.Name.Contains(request.SearchTerm) ||
                    r.Description.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var rules = await query
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var ruleDtos = _mapper.Map<List<ComplianceRuleDto>>(rules);

            return new PaginatedResult<ComplianceRuleDto>
            {
                Items = ruleDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<ComplianceRuleDto>> GetActiveComplianceRulesAsync()
        {
            var now = DateTime.UtcNow;
            var rules = await _complianceRuleRepository.FindAsync(r =>
                !r.IsDeleted &&
                r.Status == ComplianceRuleStatus.Active &&
                r.EffectiveFrom <= now &&
                (r.EffectiveTo == null || r.EffectiveTo > now));

            return _mapper.Map<List<ComplianceRuleDto>>(rules);
        }

        public async Task<List<ComplianceRuleDto>> GetComplianceRulesByTypeAsync(ComplianceRuleType ruleType)
        {
            var rules = await _complianceRuleRepository.FindAsync(r =>
                !r.IsDeleted && r.RuleType == ruleType);
            return _mapper.Map<List<ComplianceRuleDto>>(rules);
        }

        public async Task<ComplianceRuleDto> CreateComplianceRuleAsync(CreateComplianceRuleDto dto, string userId)
        {
            var rule = _mapper.Map<ComplianceRule>(dto);
            rule.Status = ComplianceRuleStatus.Draft;
            rule.CreatedBy = userId;
            rule.CreatedAt = DateTime.UtcNow;
            rule.EffectiveFrom = dto.EffectiveFrom ?? DateTime.UtcNow;

            await _complianceRuleRepository.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            // Create audit entry
            await CreateAuditEntryAsync(rule.Id, ComplianceAuditAction.Created, userId, null, 
                JsonConvert.SerializeObject(rule), "Rule created", null);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Compliance rule created: {Name} by user {UserId}", rule.Name, userId);

            return _mapper.Map<ComplianceRuleDto>(rule);
        }

        public async Task<bool> UpdateComplianceRuleAsync(int id, UpdateComplianceRuleDto dto, string userId, string? ipAddress = null)
        {
            var rule = await _complianceRuleRepository.GetByIdAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return false;
            }

            var previousState = JsonConvert.SerializeObject(rule);

            if (dto.Name != null) rule.Name = dto.Name;
            if (dto.Description != null) rule.Description = dto.Description;
            if (dto.Configuration != null) rule.Configuration = dto.Configuration;
            if (dto.Priority.HasValue) rule.Priority = dto.Priority.Value;
            if (dto.IsMandatory.HasValue) rule.IsMandatory = dto.IsMandatory.Value;
            if (dto.EffectiveFrom.HasValue) rule.EffectiveFrom = dto.EffectiveFrom.Value;
            if (dto.EffectiveTo.HasValue) rule.EffectiveTo = dto.EffectiveTo;
            if (dto.ApplicableRegions != null) rule.ApplicableRegions = dto.ApplicableRegions;
            if (dto.ApplicableServices != null) rule.ApplicableServices = dto.ApplicableServices;

            rule.ModifiedBy = userId;
            rule.UpdatedAt = DateTime.UtcNow;

            _complianceRuleRepository.Update(rule);
            
            // Create audit entry
            var newState = JsonConvert.SerializeObject(rule);
            await CreateAuditEntryAsync(id, ComplianceAuditAction.Updated, userId, previousState, 
                newState, dto.Reason, ipAddress);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Compliance rule updated: {Name} by user {UserId}", rule.Name, userId);

            return true;
        }

        public async Task<bool> DeleteComplianceRuleAsync(int id, string userId, string? reason = null, string? ipAddress = null)
        {
            var rule = await _complianceRuleRepository.GetByIdAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return false;
            }

            var previousState = JsonConvert.SerializeObject(rule);

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;
            rule.ModifiedBy = userId;

            _complianceRuleRepository.Update(rule);
            
            // Create audit entry
            await CreateAuditEntryAsync(id, ComplianceAuditAction.Deleted, userId, previousState, 
                null, reason ?? "Rule deleted", ipAddress);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Compliance rule deleted: {Name} by user {UserId}", rule.Name, userId);

            return true;
        }

        public async Task<bool> ActivateComplianceRuleAsync(int id, string userId, string? ipAddress = null)
        {
            var rule = await _complianceRuleRepository.GetByIdAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return false;
            }

            var previousState = JsonConvert.SerializeObject(rule);

            rule.Status = ComplianceRuleStatus.Active;
            rule.ModifiedBy = userId;
            rule.UpdatedAt = DateTime.UtcNow;

            _complianceRuleRepository.Update(rule);
            
            // Create audit entry
            var newState = JsonConvert.SerializeObject(rule);
            await CreateAuditEntryAsync(id, ComplianceAuditAction.Activated, userId, previousState, 
                newState, "Rule activated", ipAddress);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Compliance rule activated: {Name} by user {UserId}", rule.Name, userId);

            return true;
        }

        public async Task<bool> DeactivateComplianceRuleAsync(int id, string userId, string? reason = null, string? ipAddress = null)
        {
            var rule = await _complianceRuleRepository.GetByIdAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return false;
            }

            var previousState = JsonConvert.SerializeObject(rule);

            rule.Status = ComplianceRuleStatus.Inactive;
            rule.ModifiedBy = userId;
            rule.UpdatedAt = DateTime.UtcNow;

            _complianceRuleRepository.Update(rule);
            
            // Create audit entry
            var newState = JsonConvert.SerializeObject(rule);
            await CreateAuditEntryAsync(id, ComplianceAuditAction.Deactivated, userId, previousState, 
                newState, reason ?? "Rule deactivated", ipAddress);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Compliance rule deactivated: {Name} by user {UserId}", rule.Name, userId);

            return true;
        }

        public async Task<List<ComplianceRuleAuditDto>> GetComplianceRuleAuditTrailAsync(int ruleId)
        {
            var audits = await _auditRepository.FindAsync(a => a.ComplianceRuleId == ruleId);
            var sortedAudits = audits.OrderByDescending(a => a.CreatedAt).ToList();
            return _mapper.Map<List<ComplianceRuleAuditDto>>(sortedAudits);
        }

        public async Task<List<ComplianceRuleDto>> GetApplicableRulesAsync(string? region = null, string? service = null)
        {
            var now = DateTime.UtcNow;
            var query = _complianceRuleRepository.GetQueryable().Where(r =>
                !r.IsDeleted &&
                r.Status == ComplianceRuleStatus.Active &&
                r.EffectiveFrom <= now &&
                (r.EffectiveTo == null || r.EffectiveTo > now));

            // Filter by region if specified
            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(r =>
                    string.IsNullOrEmpty(r.ApplicableRegions) ||
                    r.ApplicableRegions.Contains(region));
            }

            // Filter by service if specified
            if (!string.IsNullOrWhiteSpace(service))
            {
                query = query.Where(r =>
                    string.IsNullOrEmpty(r.ApplicableServices) ||
                    r.ApplicableServices.Contains(service));
            }

            var rules = await query
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.Name)
                .ToListAsync();

            return _mapper.Map<List<ComplianceRuleDto>>(rules);
        }

        private async Task CreateAuditEntryAsync(int ruleId, ComplianceAuditAction action, string userId, 
            string? previousState, string? newState, string? reason, string? ipAddress)
        {
            var audit = new ComplianceRuleAudit
            {
                ComplianceRuleId = ruleId,
                Action = action,
                PerformedBy = userId,
                PreviousState = previousState,
                NewState = newState,
                Reason = reason,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _auditRepository.AddAsync(audit);
            // Note: SaveChanges is called by the calling method to ensure transactional consistency
        }
    }
}
