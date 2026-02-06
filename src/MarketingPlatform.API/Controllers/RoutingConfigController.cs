using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketingPlatform.API.Controllers
{
    /// <summary>
    /// Manages channel routing configurations for message delivery
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoutingConfigController : ControllerBase
    {
        private readonly IMessageRoutingService _messageRoutingService;
        private readonly ILogger<RoutingConfigController> _logger;

        public RoutingConfigController(
            IMessageRoutingService messageRoutingService,
            ILogger<RoutingConfigController> logger)
        {
            _messageRoutingService = messageRoutingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all routing configurations
        /// </summary>
        /// <returns>List of all routing configurations ordered by channel and priority</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<ChannelRoutingConfig>>>> GetAllConfigs()
        {
            var configs = await _messageRoutingService.GetAllConfigsAsync();

            return Ok(ApiResponse<List<ChannelRoutingConfig>>.SuccessResponse(
                configs, 
                "Routing configurations retrieved successfully"));
        }

        /// <summary>
        /// Get routing configuration by ID
        /// </summary>
        /// <param name="id">The routing configuration ID</param>
        /// <returns>The routing configuration details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ChannelRoutingConfig>>> GetConfigById(int id)
        {
            var config = await _messageRoutingService.GetConfigByIdAsync(id);
            
            if (config == null)
                return NotFound(ApiResponse<ChannelRoutingConfig>.ErrorResponse("Routing configuration not found"));

            return Ok(ApiResponse<ChannelRoutingConfig>.SuccessResponse(
                config, 
                "Routing configuration retrieved successfully"));
        }

        /// <summary>
        /// Get routing configuration by channel
        /// </summary>
        /// <param name="channel">The channel type (SMS, MMS, Email)</param>
        /// <returns>The active routing configuration for the specified channel</returns>
        [HttpGet("channel/{channel}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ChannelRoutingConfig>>> GetConfigByChannel(ChannelType channel)
        {
            var config = await _messageRoutingService.GetConfigByChannelAsync(channel);

            if (config == null)
                return NotFound(ApiResponse<ChannelRoutingConfig>.ErrorResponse(
                    $"No active routing configuration found for channel: {channel}"));

            return Ok(ApiResponse<ChannelRoutingConfig>.SuccessResponse(
                config, 
                "Routing configuration retrieved successfully"));
        }

        /// <summary>
        /// Create a new routing configuration
        /// </summary>
        /// <param name="config">The routing configuration to create</param>
        /// <returns>The created routing configuration</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ChannelRoutingConfig>>> CreateConfig(
            [FromBody] ChannelRoutingConfig config)
        {
            try
            {
                var createdConfig = await _messageRoutingService.CreateConfigAsync(config);

                return Ok(ApiResponse<ChannelRoutingConfig>.SuccessResponse(
                    createdConfig, 
                    "Routing configuration created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating routing configuration");
                return StatusCode(500, ApiResponse<ChannelRoutingConfig>.ErrorResponse(
                    "An error occurred while creating the routing configuration"));
            }
        }

        /// <summary>
        /// Update an existing routing configuration
        /// </summary>
        /// <param name="id">The routing configuration ID to update</param>
        /// <param name="updatedConfig">The updated routing configuration data</param>
        /// <returns>The updated routing configuration</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ChannelRoutingConfig>>> UpdateConfig(
            int id, 
            [FromBody] ChannelRoutingConfig updatedConfig)
        {
            try
            {
                var config = await _messageRoutingService.UpdateConfigAsync(id, updatedConfig);
                
                if (config == null)
                    return NotFound(ApiResponse<ChannelRoutingConfig>.ErrorResponse(
                        "Routing configuration not found"));

                return Ok(ApiResponse<ChannelRoutingConfig>.SuccessResponse(
                    config, 
                    "Routing configuration updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating routing configuration {ConfigId}", id);
                return StatusCode(500, ApiResponse<ChannelRoutingConfig>.ErrorResponse(
                    "An error occurred while updating the routing configuration"));
            }
        }

        /// <summary>
        /// Delete a routing configuration
        /// </summary>
        /// <param name="id">The routing configuration ID to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteConfig(int id)
        {
            try
            {
                var deleted = await _messageRoutingService.DeleteConfigAsync(id);
                
                if (!deleted)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Routing configuration not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(
                    true, 
                    "Routing configuration deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting routing configuration {ConfigId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the routing configuration"));
            }
        }

        /// <summary>
        /// Get delivery attempts for a message
        /// </summary>
        /// <param name="messageId">The campaign message ID</param>
        /// <returns>List of delivery attempts for the specified message</returns>
        [HttpGet("delivery-attempts/{messageId}")]
        public async Task<ActionResult<ApiResponse<List<MessageDeliveryAttempt>>>> GetDeliveryAttempts(int messageId)
        {
            var attempts = await _messageRoutingService.GetDeliveryAttemptsAsync(messageId);

            return Ok(ApiResponse<List<MessageDeliveryAttempt>>.SuccessResponse(
                attempts, 
                "Delivery attempts retrieved successfully"));
        }

        /// <summary>
        /// Get delivery statistics by channel
        /// </summary>
        /// <param name="channel">The channel type</param>
        /// <param name="startDate">Optional start date for statistics (defaults to 30 days ago)</param>
        /// <param name="endDate">Optional end date for statistics (defaults to now)</param>
        /// <returns>Delivery statistics for the specified channel and date range</returns>
        [HttpGet("stats/channel/{channel}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> GetChannelStats(
            ChannelType channel,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _messageRoutingService.GetChannelStatsAsync(channel, startDate, endDate);

            return Ok(ApiResponse<object>.SuccessResponse(
                stats, 
                "Channel statistics retrieved successfully"));
        }

        /// <summary>
        /// Get overall delivery statistics
        /// </summary>
        /// <param name="startDate">Optional start date for statistics (defaults to 30 days ago)</param>
        /// <param name="endDate">Optional end date for statistics (defaults to now)</param>
        /// <returns>Overall delivery statistics including breakdown by channel</returns>
        [HttpGet("stats/overall")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> GetOverallStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _messageRoutingService.GetOverallStatsAsync(startDate, endDate);

            return Ok(ApiResponse<object>.SuccessResponse(
                stats, 
                "Overall statistics retrieved successfully"));
        }
    }
}
