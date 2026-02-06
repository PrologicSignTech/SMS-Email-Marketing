using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(IContactService contactService, ILogger<ContactsController> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ContactDto>>>> GetContacts([FromQuery] PagedRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var contacts = await _contactService.GetContactsAsync(userId, request);
            return Ok(ApiResponse<PaginatedResult<ContactDto>>.SuccessResponse(contacts));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContactDto>>> GetContact(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var contact = await _contactService.GetContactByIdAsync(userId, id);
            if (contact == null)
                return NotFound(ApiResponse<ContactDto>.ErrorResponse("Contact not found"));

            return Ok(ApiResponse<ContactDto>.SuccessResponse(contact));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContactDto>>> CreateContact([FromBody] CreateContactDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var contact = await _contactService.CreateContactAsync(userId, dto);
                return Ok(ApiResponse<ContactDto>.SuccessResponse(contact, "Contact created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact");
                return BadRequest(ApiResponse<ContactDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateContact(int id, [FromBody] UpdateContactDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _contactService.UpdateContactAsync(userId, id, dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Contact not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Contact updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteContact(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _contactService.DeleteContactAsync(userId, id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Contact not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Contact deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("import/csv")]
        public async Task<ActionResult<ApiResponse<ContactImportResultDto>>> ImportCsv(IFormFile file, [FromQuery] int? groupId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ContactImportResultDto>.ErrorResponse("No file uploaded"));

            try
            {
                var result = await _contactService.ImportContactsFromCsvAsync(userId, file, groupId);
                return Ok(ApiResponse<ContactImportResultDto>.SuccessResponse(result, "CSV import completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV");
                return BadRequest(ApiResponse<ContactImportResultDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("import/excel")]
        public async Task<ActionResult<ApiResponse<ContactImportResultDto>>> ImportExcel(IFormFile file, [FromQuery] int? groupId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ContactImportResultDto>.ErrorResponse("No file uploaded"));

            try
            {
                var result = await _contactService.ImportContactsFromExcelAsync(userId, file, groupId);
                return Ok(ApiResponse<ContactImportResultDto>.SuccessResponse(result, "Excel import completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing Excel");
                return BadRequest(ApiResponse<ContactImportResultDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("export/csv")]
        public async Task<IActionResult> ExportCsv([FromBody] List<int>? contactIds = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _contactService.ExportContactsToCsvAsync(userId, contactIds);
                return File(data, "text/csv", $"contacts_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to CSV");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel([FromBody] List<int>? contactIds = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _contactService.ExportContactsToExcelAsync(userId, contactIds);
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"contacts_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ContactDto>>>> SearchContacts([FromQuery] string searchTerm)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest(ApiResponse<List<ContactDto>>.ErrorResponse("Search term is required"));

            try
            {
                var contacts = await _contactService.SearchContactsAsync(userId, searchTerm);
                return Ok(ApiResponse<List<ContactDto>>.SuccessResponse(contacts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contacts");
                return BadRequest(ApiResponse<List<ContactDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("check-duplicates")]
        public async Task<ActionResult<ApiResponse<DuplicateCheckResultDto>>> CheckDuplicates([FromBody] CheckDuplicateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _contactService.CheckForDuplicatesAsync(userId, dto);
                return Ok(ApiResponse<DuplicateCheckResultDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicates");
                return BadRequest(ApiResponse<DuplicateCheckResultDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("duplicates/report")]
        public async Task<ActionResult<ApiResponse<DuplicateReportDto>>> GetDuplicatesReport()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var report = await _contactService.GetDuplicateReportAsync(userId);
                return Ok(ApiResponse<DuplicateReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating duplicate report");
                return BadRequest(ApiResponse<DuplicateReportDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("duplicates/resolve")]
        public async Task<ActionResult<ApiResponse<ResolveDuplicateResultDto>>> ResolveDuplicates([FromBody] ResolveDuplicateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _contactService.ResolveDuplicatesAsync(userId, dto);
                if (!result.Success)
                    return BadRequest(ApiResponse<ResolveDuplicateResultDto>.ErrorResponse(result.Message));

                return Ok(ApiResponse<ResolveDuplicateResultDto>.SuccessResponse(result, result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving duplicates");
                return BadRequest(ApiResponse<ResolveDuplicateResultDto>.ErrorResponse(ex.Message));
            }
        }
    }
}
