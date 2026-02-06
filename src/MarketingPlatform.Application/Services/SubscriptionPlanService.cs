using AutoMapper;
using MarketingPlatform.Application.DTOs.Subscription;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionPlanService> _logger;

        public SubscriptionPlanService(
            IRepository<SubscriptionPlan> planRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SubscriptionPlanService> logger)
        {
            _planRepository = planRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);
            
            if (plan == null)
                return null;

            return MapToPlanDto(plan);
        }

        public async Task<List<SubscriptionPlanDto>> GetAllPlansAsync(bool includeInactive = false)
        {
            var plans = includeInactive
                ? await _planRepository.FindAsync(p => !p.IsDeleted)
                : await _planRepository.FindAsync(p => p.IsActive && !p.IsDeleted);

            return plans.OrderBy(p => p.PriceMonthly).Select(MapToPlanDto).ToList();
        }

        public async Task<List<SubscriptionPlanDto>> GetVisiblePlansAsync()
        {
            var plans = await _planRepository.FindAsync(p => 
                p.IsActive && p.IsVisible && !p.IsDeleted);

            return plans.OrderBy(p => p.PriceMonthly).Select(MapToPlanDto).ToList();
        }

        public async Task<List<SubscriptionPlanDto>> GetLandingPagePlansAsync()
        {
            var plans = await _planRepository.GetQueryable()
                .Where(p => p.IsActive && p.IsVisible && p.ShowOnLanding && !p.IsDeleted)
                .Include(p => p.PlanFeatureMappings)
                    .ThenInclude(pfm => pfm.Feature)
                .OrderBy(p => p.PriceMonthly)
                .ToListAsync();

            return plans.Select(MapToPlanDto).ToList();
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
        {
            var plan = _mapper.Map<SubscriptionPlan>(dto);
            plan.Features = SerializeJson(dto.Features);

            await _planRepository.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created subscription plan {PlanId} with name {PlanName}", plan.Id, plan.Name);

            return MapToPlanDto(plan);
        }

        public async Task<SubscriptionPlanDto?> UpdatePlanAsync(int planId, UpdateSubscriptionPlanDto dto)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Attempted to update non-existent plan {PlanId}", planId);
                return null;
            }

            if (dto.Name != null)
                plan.Name = dto.Name;
            
            if (dto.Description != null)
                plan.Description = dto.Description;
            
            if (dto.PriceMonthly.HasValue)
                plan.PriceMonthly = dto.PriceMonthly.Value;
            
            if (dto.PriceYearly.HasValue)
                plan.PriceYearly = dto.PriceYearly.Value;
            
            if (dto.SMSLimit.HasValue)
                plan.SMSLimit = dto.SMSLimit.Value;
            
            if (dto.MMSLimit.HasValue)
                plan.MMSLimit = dto.MMSLimit.Value;
            
            if (dto.EmailLimit.HasValue)
                plan.EmailLimit = dto.EmailLimit.Value;
            
            if (dto.ContactLimit.HasValue)
                plan.ContactLimit = dto.ContactLimit.Value;
            
            if (dto.Features != null)
                plan.Features = SerializeJson(dto.Features);
            
            if (dto.IsActive.HasValue)
                plan.IsActive = dto.IsActive.Value;
            
            if (dto.IsVisible.HasValue)
                plan.IsVisible = dto.IsVisible.Value;
            
            if (dto.ShowOnLanding.HasValue)
                plan.ShowOnLanding = dto.ShowOnLanding.Value;

            plan.UpdatedAt = DateTime.UtcNow;

            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated subscription plan {PlanId}", planId);

            return MapToPlanDto(plan);
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Attempted to delete non-existent plan {PlanId}", planId);
                return false;
            }

            plan.IsDeleted = true;
            plan.UpdatedAt = DateTime.UtcNow;

            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Soft deleted subscription plan {PlanId}", planId);

            return true;
        }

        public async Task<Dictionary<string, object>> GetPlanFeaturesAsync(int planId)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Plan {PlanId} not found when retrieving features", planId);
                throw new ArgumentException($"Plan with ID {planId} not found");
            }

            return DeserializeJson<Dictionary<string, object>>(plan.Features) ?? new Dictionary<string, object>();
        }

        public async Task<bool> UpdatePlanFeaturesAsync(int planId, Dictionary<string, object> features)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Attempted to update features for non-existent plan {PlanId}", planId);
                return false;
            }

            plan.Features = SerializeJson(features);
            plan.UpdatedAt = DateTime.UtcNow;

            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated features for subscription plan {PlanId}", planId);

            return true;
        }

        public async Task<PlanLimitsDto> GetPlanLimitsAsync(int planId)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Plan {PlanId} not found when retrieving limits", planId);
                throw new ArgumentException($"Plan with ID {planId} not found");
            }

            return new PlanLimitsDto
            {
                SMSLimit = plan.SMSLimit,
                MMSLimit = plan.MMSLimit,
                EmailLimit = plan.EmailLimit,
                ContactLimit = plan.ContactLimit
            };
        }

        public async Task<bool> SetPlanVisibilityAsync(int planId, bool isVisible)
        {
            var plan = await _planRepository.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted);

            if (plan == null)
            {
                _logger.LogWarning("Attempted to set visibility for non-existent plan {PlanId}", planId);
                return false;
            }

            plan.IsVisible = isVisible;
            plan.UpdatedAt = DateTime.UtcNow;

            _planRepository.Update(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Set visibility to {IsVisible} for subscription plan {PlanId}", isVisible, planId);

            return true;
        }

        public async Task<List<SubscriptionPlanDto>> GetEligibleUpgradesAsync(int currentPlanId)
        {
            var currentPlan = await _planRepository.FirstOrDefaultAsync(p => 
                p.Id == currentPlanId && !p.IsDeleted);

            if (currentPlan == null)
            {
                _logger.LogWarning("Current plan {PlanId} not found when retrieving eligible upgrades", currentPlanId);
                throw new ArgumentException($"Plan with ID {currentPlanId} not found");
            }

            var eligiblePlans = await _planRepository.FindAsync(p =>
                p.IsActive && !p.IsDeleted && p.PriceMonthly > currentPlan.PriceMonthly);

            return eligiblePlans.OrderBy(p => p.PriceMonthly).Select(MapToPlanDto).ToList();
        }

        public async Task<bool> CanUpgradeToAsync(int currentPlanId, int targetPlanId)
        {
            var currentPlan = await _planRepository.FirstOrDefaultAsync(p => 
                p.Id == currentPlanId && !p.IsDeleted);
            
            var targetPlan = await _planRepository.FirstOrDefaultAsync(p => 
                p.Id == targetPlanId && !p.IsDeleted);

            if (currentPlan == null || targetPlan == null)
            {
                _logger.LogWarning("Plan not found: CurrentPlanId={CurrentPlanId}, TargetPlanId={TargetPlanId}", 
                    currentPlanId, targetPlanId);
                return false;
            }

            return targetPlan.PriceMonthly > currentPlan.PriceMonthly;
        }

        public async Task<bool> CanDowngradeToAsync(int currentPlanId, int targetPlanId)
        {
            var currentPlan = await _planRepository.FirstOrDefaultAsync(p => 
                p.Id == currentPlanId && !p.IsDeleted);
            
            var targetPlan = await _planRepository.FirstOrDefaultAsync(p => 
                p.Id == targetPlanId && !p.IsDeleted);

            if (currentPlan == null || targetPlan == null)
            {
                _logger.LogWarning("Plan not found: CurrentPlanId={CurrentPlanId}, TargetPlanId={TargetPlanId}", 
                    currentPlanId, targetPlanId);
                return false;
            }

            return targetPlan.PriceMonthly < currentPlan.PriceMonthly;
        }

        private SubscriptionPlanDto MapToPlanDto(SubscriptionPlan plan)
        {
            var dto = _mapper.Map<SubscriptionPlanDto>(plan);
            dto.Features = DeserializeJson<Dictionary<string, object>>(plan.Features);
            dto.PlanCategory = plan.PlanCategory ?? "Standard";
            dto.IsMostPopular = plan.IsMostPopular;

            if (plan.PlanFeatureMappings != null && plan.PlanFeatureMappings.Any())
            {
                dto.PlanFeatures = plan.PlanFeatureMappings
                    .OrderBy(pfm => pfm.DisplayOrder)
                    .Select(pfm => new PlanFeatureDto
                    {
                        Id = pfm.Id,
                        FeatureId = pfm.FeatureId,
                        FeatureName = pfm.Feature?.Name ?? "",
                        FeatureDescription = pfm.Feature?.Description,
                        FeatureValue = pfm.FeatureValue,
                        IsIncluded = pfm.IsIncluded,
                        DisplayOrder = pfm.DisplayOrder
                    })
                    .ToList();
            }

            return dto;
        }

        private T? DeserializeJson<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON: {Json}", json);
                return null;
            }
        }

        private string? SerializeJson<T>(T? obj) where T : class
        {
            if (obj == null)
                return null;

            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize object to JSON");
                return null;
            }
        }
    }
}
