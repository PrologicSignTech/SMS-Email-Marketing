using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ICampaignSchedulerService
    {
        /// <summary>
        /// Schedule a campaign to run at a specific date/time with timezone support
        /// </summary>
        Task ScheduleCampaignAsync(int campaignId, DateTime scheduledDateTime, string? timeZone = null);

        /// <summary>
        /// Schedule a recurring campaign
        /// </summary>
        Task ScheduleRecurringCampaignAsync(int campaignId, string cronExpression, string? timeZone = null);

        /// <summary>
        /// Schedule a drip campaign with step-by-step delays
        /// </summary>
        Task ScheduleDripCampaignAsync(int campaignId, int workflowId);

        /// <summary>
        /// Cancel a scheduled campaign
        /// </summary>
        Task CancelScheduledCampaignAsync(int campaignId);

        /// <summary>
        /// Process a scheduled campaign (executed by Hangfire)
        /// </summary>
        Task ProcessScheduledCampaignAsync(int campaignId);

        /// <summary>
        /// Process a recurring campaign iteration
        /// </summary>
        Task ProcessRecurringCampaignAsync(int campaignId);
    }
}
