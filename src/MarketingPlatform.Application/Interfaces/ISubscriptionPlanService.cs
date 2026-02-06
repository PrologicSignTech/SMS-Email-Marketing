using MarketingPlatform.Application.DTOs.Subscription;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ISubscriptionPlanService
    {
        // Plan CRUD
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId);
        Task<List<SubscriptionPlanDto>> GetAllPlansAsync(bool includeInactive = false);
        Task<List<SubscriptionPlanDto>> GetVisiblePlansAsync();
        Task<List<SubscriptionPlanDto>> GetLandingPagePlansAsync();
        Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto);
        Task<SubscriptionPlanDto?> UpdatePlanAsync(int planId, UpdateSubscriptionPlanDto dto);
        Task<bool> DeletePlanAsync(int planId);
        
        // Feature & Limit Management
        Task<Dictionary<string, object>> GetPlanFeaturesAsync(int planId);
        Task<bool> UpdatePlanFeaturesAsync(int planId, Dictionary<string, object> features);
        Task<PlanLimitsDto> GetPlanLimitsAsync(int planId);
        
        // Visibility Control
        Task<bool> SetPlanVisibilityAsync(int planId, bool isVisible);
        
        // Upgrade/Downgrade Rules
        Task<List<SubscriptionPlanDto>> GetEligibleUpgradesAsync(int currentPlanId);
        Task<bool> CanUpgradeToAsync(int currentPlanId, int targetPlanId);
        Task<bool> CanDowngradeToAsync(int currentPlanId, int targetPlanId);
    }
}
