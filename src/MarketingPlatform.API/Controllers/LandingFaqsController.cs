using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/landingfaqs")]
    public class LandingFaqsController : ControllerBase
    {
        private readonly ILandingFaqService _landingFaqService;
        private readonly ILogger<LandingFaqsController> _logger;

        public LandingFaqsController(
            ILandingFaqService landingFaqService,
            ILogger<LandingFaqsController> logger)
        {
            _landingFaqService = landingFaqService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active landing FAQs for display on landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<LandingFaq>>>> GetLandingFaqs()
        {
            try
            {
                var faqs = await _landingFaqService.GetAllActiveAsync();
                var faqList = faqs.ToList();

                _logger.LogInformation("Retrieved {Count} landing FAQs", faqList.Count);

                return Ok(ApiResponse<List<LandingFaq>>.SuccessResponse(faqList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving landing FAQs");
                return BadRequest(ApiResponse<List<LandingFaq>>.ErrorResponse(
                    "Failed to retrieve landing FAQs",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific landing FAQ by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingFaq>>> GetById(int id)
        {
            try
            {
                var faq = await _landingFaqService.GetByIdAsync(id);

                if (faq == null)
                    return NotFound(ApiResponse<LandingFaq>.ErrorResponse("FAQ not found"));

                return Ok(ApiResponse<LandingFaq>.SuccessResponse(faq));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ {FaqId}", id);
                return BadRequest(ApiResponse<LandingFaq>.ErrorResponse(
                    "Failed to retrieve FAQ",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new landing FAQ (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingFaq>>> Create([FromBody] LandingFaq faq)
        {
            try
            {
                var createdFaq = await _landingFaqService.CreateAsync(faq);

                return Ok(ApiResponse<LandingFaq>.SuccessResponse(
                    createdFaq,
                    "FAQ created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating landing FAQ");
                return BadRequest(ApiResponse<LandingFaq>.ErrorResponse(
                    "Failed to create FAQ",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a landing FAQ (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LandingFaq>>> Update(int id, [FromBody] LandingFaq faq)
        {
            try
            {
                var updatedFaq = await _landingFaqService.UpdateAsync(id, faq);

                return Ok(ApiResponse<LandingFaq>.SuccessResponse(
                    updatedFaq,
                    "FAQ updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<LandingFaq>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ {FaqId}", id);
                return BadRequest(ApiResponse<LandingFaq>.ErrorResponse(
                    "Failed to update FAQ",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a landing FAQ (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _landingFaqService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "FAQ deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FAQ {FaqId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete FAQ",
                    new List<string> { ex.Message }));
            }
        }
    }
}
