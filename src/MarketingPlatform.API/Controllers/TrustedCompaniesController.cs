using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/trustedcompanies")]
    public class TrustedCompaniesController : ControllerBase
    {
        private readonly ITrustedCompanyService _trustedCompanyService;
        private readonly ILogger<TrustedCompaniesController> _logger;

        public TrustedCompaniesController(
            ITrustedCompanyService trustedCompanyService,
            ILogger<TrustedCompaniesController> logger)
        {
            _trustedCompanyService = trustedCompanyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active trusted companies for landing page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TrustedCompany>>>> GetTrustedCompanies()
        {
            try
            {
                var companies = await _trustedCompanyService.GetAllActiveAsync();
                var companyList = companies.ToList();

                _logger.LogInformation("Retrieved {Count} trusted companies", companyList.Count);

                return Ok(ApiResponse<List<TrustedCompany>>.SuccessResponse(companyList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trusted companies");
                return Ok(ApiResponse<List<TrustedCompany>>.SuccessResponse(new List<TrustedCompany>()));
            }
        }

        /// <summary>
        /// Get a specific trusted company by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TrustedCompany>>> GetById(int id)
        {
            try
            {
                var company = await _trustedCompanyService.GetByIdAsync(id);

                if (company == null)
                    return NotFound(ApiResponse<TrustedCompany>.ErrorResponse("Company not found"));

                return Ok(ApiResponse<TrustedCompany>.SuccessResponse(company));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company {CompanyId}", id);
                return BadRequest(ApiResponse<TrustedCompany>.ErrorResponse(
                    "Failed to retrieve company",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new trusted company (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<TrustedCompany>>> Create([FromBody] TrustedCompany company)
        {
            try
            {
                var createdCompany = await _trustedCompanyService.CreateAsync(company);

                return Ok(ApiResponse<TrustedCompany>.SuccessResponse(
                    createdCompany,
                    "Company created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trusted company");
                return BadRequest(ApiResponse<TrustedCompany>.ErrorResponse(
                    "Failed to create company",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update a trusted company (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<TrustedCompany>>> Update(int id, [FromBody] TrustedCompany company)
        {
            try
            {
                var updatedCompany = await _trustedCompanyService.UpdateAsync(id, company);

                return Ok(ApiResponse<TrustedCompany>.SuccessResponse(
                    updatedCompany,
                    "Company updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<TrustedCompany>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", id);
                return BadRequest(ApiResponse<TrustedCompany>.ErrorResponse(
                    "Failed to update company",
                    new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a trusted company (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var result = await _trustedCompanyService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Company deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", id);
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Failed to delete company",
                    new List<string> { ex.Message }));
            }
        }
    }
}
