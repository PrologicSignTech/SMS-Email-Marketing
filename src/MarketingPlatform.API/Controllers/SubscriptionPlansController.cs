using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Subscription;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/subscriptionplans")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly ILogger<SubscriptionPlansController> _logger;

        public SubscriptionPlansController(
            ISubscriptionPlanService subscriptionPlanService,
            ILogger<SubscriptionPlansController> logger)
        {
            _subscriptionPlanService = subscriptionPlanService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                var plans = await _subscriptionPlanService.GetAllPlansAsync(includeInactive);
                return Ok(ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(plans));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subscription plans");
                return BadRequest(ApiResponse<List<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve subscription plans", new List<string> { ex.Message }));
            }
        }

        [HttpGet("visible")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetVisible()
        {
            try
            {
                var plans = await _subscriptionPlanService.GetVisiblePlansAsync();
                return Ok(ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(plans));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visible subscription plans");
                return BadRequest(ApiResponse<List<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve visible plans", new List<string> { ex.Message }));
            }
        }

        [HttpGet("landing")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetLandingPagePlans()
        {
            try
            {
                var plans = await _subscriptionPlanService.GetLandingPagePlansAsync();
                return Ok(ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(plans));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting landing page subscription plans");
                return BadRequest(ApiResponse<List<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve landing page plans", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> GetById(int id)
        {
            try
            {
                var plan = await _subscriptionPlanService.GetPlanByIdAsync(id);
                if (plan == null)
                    return NotFound(ApiResponse<SubscriptionPlanDto>.ErrorResponse("Subscription plan not found"));

                return Ok(ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription plan {PlanId}", id);
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to retrieve subscription plan", new List<string> { ex.Message }));
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> Create([FromBody] CreateSubscriptionPlanDto dto)
        {
            try
            {
                var plan = await _subscriptionPlanService.CreatePlanAsync(dto);
                return Ok(ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan, "Subscription plan created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription plan");
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to create subscription plan", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> Update(int id, [FromBody] UpdateSubscriptionPlanDto dto)
        {
            try
            {
                var plan = await _subscriptionPlanService.UpdatePlanAsync(id, dto);
                if (plan == null)
                    return NotFound(ApiResponse<SubscriptionPlanDto>.ErrorResponse("Subscription plan not found"));

                return Ok(ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan, "Subscription plan updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription plan {PlanId}", id);
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to update subscription plan", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _subscriptionPlanService.DeletePlanAsync(id);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete subscription plan. Plan not found or in use."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Subscription plan deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription plan {PlanId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to delete subscription plan", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}/visibility")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetVisibility(int id, [FromBody] bool isVisible)
        {
            try
            {
                var result = await _subscriptionPlanService.SetPlanVisibilityAsync(id, isVisible);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update plan visibility. Plan not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Plan visibility updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plan visibility for {PlanId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update plan visibility", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}/show-on-landing")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> SetShowOnLanding(int id, [FromBody] bool showOnLanding)
        {
            try
            {
                var plan = await _subscriptionPlanService.GetPlanByIdAsync(id);
                if (plan == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Subscription plan not found"));

                var updateDto = new UpdateSubscriptionPlanDto
                {
                    ShowOnLanding = showOnLanding
                };

                var result = await _subscriptionPlanService.UpdatePlanAsync(id, updateDto);
                if (result == null)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update ShowOnLanding flag"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, $"Plan {(showOnLanding ? "will now" : "will no longer")} be displayed on the landing page"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ShowOnLanding flag for {PlanId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to update ShowOnLanding flag", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}/features")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetFeatures(int id)
        {
            try
            {
                var features = await _subscriptionPlanService.GetPlanFeaturesAsync(id);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResponse(features));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan features for {PlanId}", id);
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResponse("Failed to retrieve plan features", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{currentPlanId}/eligible-upgrades")]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetEligibleUpgrades(int currentPlanId)
        {
            try
            {
                var plans = await _subscriptionPlanService.GetEligibleUpgradesAsync(currentPlanId);
                return Ok(ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(plans));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting eligible upgrades for plan {PlanId}", currentPlanId);
                return BadRequest(ApiResponse<List<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve eligible upgrades", new List<string> { ex.Message }));
            }
        }
    }
}
