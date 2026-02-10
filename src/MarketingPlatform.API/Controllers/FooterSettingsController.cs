using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/footersettings")]
    public class FooterSettingsController : ControllerBase
    {
        private readonly IFooterSettingsService _footerSettingsService;
        private readonly ILogger<FooterSettingsController> _logger;

        public FooterSettingsController(
            IFooterSettingsService footerSettingsService,
            ILogger<FooterSettingsController> logger)
        {
            _footerSettingsService = footerSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Get active footer settings for landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<FooterSettings>>> GetFooterSettings()
        {
            try
            {
                var settings = await _footerSettingsService.GetActiveAsync();

                if (settings == null)
                {
                    // Return default settings if none exist
                    return Ok(ApiResponse<FooterSettings>.SuccessResponse(new FooterSettings
                    {
                        CompanyName = "Marketing Platform",
                        CompanyDescription = "Enterprise-grade SMS, MMS & Email marketing platform. Reach your customers where they are and grow your business exponentially.",
                        AddressLine1 = "123 Business Avenue, Suite 100",
                        AddressLine2 = "New York, NY 10001, USA",
                        Phone = "+1 (234) 567-890",
                        Email = "support@marketingplatform.com",
                        BusinessHours = "Mon - Fri: 9:00 AM - 6:00 PM EST",
                        ShowNewsletter = true,
                        ShowMap = true,
                        IsActive = true
                    }));
                }

                return Ok(ApiResponse<FooterSettings>.SuccessResponse(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving footer settings");
                // Return default settings on error so landing page still renders
                return Ok(ApiResponse<FooterSettings>.SuccessResponse(new FooterSettings
                {
                    CompanyName = "Marketing Platform",
                    CompanyDescription = "Enterprise-grade SMS, MMS & Email marketing platform.",
                    AddressLine1 = "123 Business Avenue, Suite 100",
                    AddressLine2 = "New York, NY 10001, USA",
                    Phone = "+1 (234) 567-890",
                    Email = "support@marketingplatform.com",
                    BusinessHours = "Mon - Fri: 9:00 AM - 6:00 PM EST",
                    ShowNewsletter = true,
                    ShowMap = true,
                    IsActive = true
                }));
            }
        }

        /// <summary>
        /// Get footer settings by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<FooterSettings>>> GetById(int id)
        {
            try
            {
                var settings = await _footerSettingsService.GetByIdAsync(id);

                if (settings == null)
                    return NotFound(ApiResponse<FooterSettings>.ErrorResponse("Settings not found"));

                return Ok(ApiResponse<FooterSettings>.SuccessResponse(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving footer settings {Id}", id);
                return BadRequest(ApiResponse<FooterSettings>.ErrorResponse(
                    "Failed to retrieve settings",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create footer settings (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<FooterSettings>>> Create([FromBody] FooterSettings settings)
        {
            try
            {
                var createdSettings = await _footerSettingsService.CreateAsync(settings);

                return Ok(ApiResponse<FooterSettings>.SuccessResponse(
                    createdSettings,
                    "Footer settings created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating footer settings");
                return BadRequest(ApiResponse<FooterSettings>.ErrorResponse(
                    "Failed to create settings",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update footer settings (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<FooterSettings>>> Update(int id, [FromBody] FooterSettings settings)
        {
            try
            {
                var updatedSettings = await _footerSettingsService.UpdateAsync(id, settings);

                return Ok(ApiResponse<FooterSettings>.SuccessResponse(
                    updatedSettings,
                    "Footer settings updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<FooterSettings>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating footer settings {Id}", id);
                return BadRequest(ApiResponse<FooterSettings>.ErrorResponse(
                    "Failed to update settings",
                    new List<string> { ex.Message }));
            }
        }
    }
}
