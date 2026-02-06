using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IEventTriggerService
    {
        /// <summary>
        /// Trigger workflows based on an event
        /// </summary>
        Task TriggerEventAsync(EventType eventType, int contactId, Dictionary<string, object>? eventData = null);

        /// <summary>
        /// Check for inactivity and trigger workflows
        /// </summary>
        Task CheckInactivityTriggersAsync();

        /// <summary>
        /// Process keyword-based triggers
        /// </summary>
        Task ProcessKeywordTriggerAsync(string keyword, int contactId);

        /// <summary>
        /// Register a custom event trigger
        /// </summary>
        Task RegisterCustomEventAsync(string eventName, int contactId, Dictionary<string, object>? eventData = null);
    }
}
