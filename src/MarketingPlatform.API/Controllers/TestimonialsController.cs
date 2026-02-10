using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/testimonials")]
    public class TestimonialsController : ControllerBase
    {
        private readonly ITestimonialService _testimonialService;
        private readonly ILogger<TestimonialsController> _logger;

        public TestimonialsController(
            ITestimonialService testimonialService,
            ILogger<TestimonialsController> logger)
        {
            _testimonialService = testimonialService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active testimonials for landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Testimonial>>>> GetTestimonials()
        {
            try
            {
                var testimonials = await _testimonialService.GetAllActiveAsync();
                var testimonialList = testimonials.ToList();

                _logger.LogInformation("Retrieved {Count} testimonials", testimonialList.Count);

                return Ok(ApiResponse<List<Testimonial>>.SuccessResponse(testimonialList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving testimonials");
                return Ok(ApiResponse<List<Testimonial>>.SuccessResponse(new List<Testimonial>()));
            }
        }

        /// <summary>
        /// Get a specific testimonial by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Testimonial>>> GetById(int id)
        {
            try
            {
                var testimonial = await _testimonialService.GetByIdAsync(id);

                if (testimonial == null)
                    return NotFound(ApiResponse<Testimonial>.ErrorResponse("Testimonial not found"));

                return Ok(ApiResponse<Testimonial>.SuccessResponse(testimonial));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving testimonial {TestimonialId}", id);
                return BadRequest(ApiResponse<Testimonial>.ErrorResponse(
                    "Failed to retrieve testimonial",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new testimonial (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<Testimonial>>> Create([FromBody] Testimonial testimonial)
        {
            try
            {
                var createdTestimonial = await _testimonialService.CreateAsync(testimonial);

                return Ok(ApiResponse<Testimonial>.SuccessResponse(
                    createdTestimonial,
                    "Testimonial created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating testimonial");
                return BadRequest(ApiResponse<Testimonial>.ErrorResponse(
                    "Failed to create testimonial",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a testimonial (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<Testimonial>>> Update(int id, [FromBody] Testimonial testimonial)
        {
            try
            {
                var updatedTestimonial = await _testimonialService.UpdateAsync(id, testimonial);

                return Ok(ApiResponse<Testimonial>.SuccessResponse(
                    updatedTestimonial,
                    "Testimonial updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<Testimonial>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating testimonial {TestimonialId}", id);
                return BadRequest(ApiResponse<Testimonial>.ErrorResponse(
                    "Failed to update testimonial",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a testimonial (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _testimonialService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Testimonial deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial {TestimonialId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete testimonial",
                    new List<string> { ex.Message }));
            }
        }
    }
}
