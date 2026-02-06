using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Analytics;
using MarketingPlatform.Application.Interfaces;
using System.Security.Claims;

namespace MarketingPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IReportExportService _exportService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IReportExportService exportService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _exportService = exportService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard summary with overall statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboard([FromQuery] ReportFilterDto? filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var dashboard = await _analyticsService.GetDashboardSummaryAsync(userId, filter);
                return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, ApiResponse<DashboardSummaryDto>.ErrorResponse("Failed to retrieve dashboard"));
            }
        }

        /// <summary>
        /// Get campaign performance analytics with filters
        /// </summary>
        [HttpGet("campaigns/performance")]
        public async Task<ActionResult<ApiResponse<List<CampaignPerformanceDto>>>> GetCampaignPerformance([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var performance = await _analyticsService.GetCampaignPerformanceAsync(userId, filter);
                return Ok(ApiResponse<List<CampaignPerformanceDto>>.SuccessResponse(performance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign performance");
                return StatusCode(500, ApiResponse<List<CampaignPerformanceDto>>.ErrorResponse("Failed to retrieve campaign performance"));
            }
        }

        /// <summary>
        /// Get specific campaign performance by ID
        /// </summary>
        [HttpGet("campaigns/{campaignId}/performance")]
        public async Task<ActionResult<ApiResponse<CampaignPerformanceDto>>> GetCampaignPerformanceById(int campaignId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var performance = await _analyticsService.GetCampaignPerformanceByIdAsync(userId, campaignId);
                if (performance == null)
                    return NotFound(ApiResponse<CampaignPerformanceDto>.ErrorResponse("Campaign not found"));

                return Ok(ApiResponse<CampaignPerformanceDto>.SuccessResponse(performance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign performance for ID {CampaignId}", campaignId);
                return StatusCode(500, ApiResponse<CampaignPerformanceDto>.ErrorResponse("Failed to retrieve campaign performance"));
            }
        }

        /// <summary>
        /// Get contact engagement history with filters
        /// </summary>
        [HttpGet("contacts/engagement")]
        public async Task<ActionResult<ApiResponse<List<ContactEngagementHistoryDto>>>> GetContactEngagement([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var engagement = await _analyticsService.GetContactEngagementHistoryAsync(userId, filter);
                return Ok(ApiResponse<List<ContactEngagementHistoryDto>>.SuccessResponse(engagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact engagement");
                return StatusCode(500, ApiResponse<List<ContactEngagementHistoryDto>>.ErrorResponse("Failed to retrieve contact engagement"));
            }
        }

        /// <summary>
        /// Get specific contact engagement history by ID
        /// </summary>
        [HttpGet("contacts/{contactId}/engagement")]
        public async Task<ActionResult<ApiResponse<ContactEngagementHistoryDto>>> GetContactEngagementById(int contactId, [FromQuery] ReportFilterDto? filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var engagement = await _analyticsService.GetContactEngagementByIdAsync(userId, contactId, filter);
                if (engagement == null)
                    return NotFound(ApiResponse<ContactEngagementHistoryDto>.ErrorResponse("Contact not found"));

                return Ok(ApiResponse<ContactEngagementHistoryDto>.SuccessResponse(engagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact engagement for ID {ContactId}", contactId);
                return StatusCode(500, ApiResponse<ContactEngagementHistoryDto>.ErrorResponse("Failed to retrieve contact engagement"));
            }
        }

        /// <summary>
        /// Get conversion tracking for a specific campaign
        /// </summary>
        [HttpGet("campaigns/{campaignId}/conversions")]
        public async Task<ActionResult<ApiResponse<ConversionTrackingDto>>> GetConversionTracking(int campaignId, [FromQuery] ReportFilterDto? filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var tracking = await _analyticsService.GetConversionTrackingAsync(userId, campaignId, filter);
                if (tracking == null)
                    return NotFound(ApiResponse<ConversionTrackingDto>.ErrorResponse("Campaign not found"));

                return Ok(ApiResponse<ConversionTrackingDto>.SuccessResponse(tracking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversion tracking for campaign {CampaignId}", campaignId);
                return StatusCode(500, ApiResponse<ConversionTrackingDto>.ErrorResponse("Failed to retrieve conversion tracking"));
            }
        }

        /// <summary>
        /// Get conversion tracking for multiple campaigns with filters
        /// </summary>
        [HttpGet("conversions")]
        public async Task<ActionResult<ApiResponse<List<ConversionTrackingDto>>>> GetConversionsForCampaigns([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var tracking = await _analyticsService.GetConversionTrackingForCampaignsAsync(userId, filter);
                return Ok(ApiResponse<List<ConversionTrackingDto>>.SuccessResponse(tracking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversion tracking");
                return StatusCode(500, ApiResponse<List<ConversionTrackingDto>>.ErrorResponse("Failed to retrieve conversion tracking"));
            }
        }

        /// <summary>
        /// Export campaign performance report to CSV
        /// </summary>
        [HttpGet("campaigns/performance/export/csv")]
        public async Task<IActionResult> ExportCampaignPerformanceToCsv([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetCampaignPerformanceAsync(userId, filter);
                var csvData = await _exportService.ExportToCsvAsync(data);
                
                return File(csvData, "text/csv", $"campaign-performance-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting campaign performance to CSV");
                return StatusCode(500, "Failed to export data");
            }
        }

        /// <summary>
        /// Export campaign performance report to Excel
        /// </summary>
        [HttpGet("campaigns/performance/export/excel")]
        public async Task<IActionResult> ExportCampaignPerformanceToExcel([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetCampaignPerformanceAsync(userId, filter);
                var excelData = await _exportService.ExportToExcelAsync(data, "Campaign Performance");
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"campaign-performance-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting campaign performance to Excel");
                return StatusCode(500, "Failed to export data");
            }
        }

        /// <summary>
        /// Export contact engagement report to CSV
        /// </summary>
        [HttpGet("contacts/engagement/export/csv")]
        public async Task<IActionResult> ExportContactEngagementToCsv([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetContactEngagementHistoryAsync(userId, filter);
                var csvData = await _exportService.ExportToCsvAsync(data);
                
                return File(csvData, "text/csv", $"contact-engagement-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contact engagement to CSV");
                return StatusCode(500, "Failed to export data");
            }
        }

        /// <summary>
        /// Export contact engagement report to Excel
        /// </summary>
        [HttpGet("contacts/engagement/export/excel")]
        public async Task<IActionResult> ExportContactEngagementToExcel([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetContactEngagementHistoryAsync(userId, filter);
                var excelData = await _exportService.ExportToExcelAsync(data, "Contact Engagement");
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"contact-engagement-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contact engagement to Excel");
                return StatusCode(500, "Failed to export data");
            }
        }

        /// <summary>
        /// Export conversion tracking report to CSV
        /// </summary>
        [HttpGet("conversions/export/csv")]
        public async Task<IActionResult> ExportConversionsToCsv([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetConversionTrackingForCampaignsAsync(userId, filter);
                var csvData = await _exportService.ExportToCsvAsync(data);
                
                return File(csvData, "text/csv", $"conversions-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting conversions to CSV");
                return StatusCode(500, "Failed to export data");
            }
        }

        /// <summary>
        /// Export conversion tracking report to Excel
        /// </summary>
        [HttpGet("conversions/export/excel")]
        public async Task<IActionResult> ExportConversionsToExcel([FromQuery] ReportFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var data = await _analyticsService.GetConversionTrackingForCampaignsAsync(userId, filter);
                var excelData = await _exportService.ExportToExcelAsync(data, "Conversions");
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"conversions-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting conversions to Excel");
                return StatusCode(500, "Failed to export data");
            }
        }
    }
}
