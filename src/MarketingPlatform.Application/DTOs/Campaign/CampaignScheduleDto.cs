using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Campaign
{
    public class CampaignScheduleDto
    {
        public ScheduleType ScheduleType { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? RecurrenceRule { get; set; }
        public bool TimeZoneAware { get; set; }
        public string? PreferredTimeZone { get; set; }
    }
}
