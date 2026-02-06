using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Entities
{
    public class CampaignSchedule : BaseEntity
    {
        public int CampaignId { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public DateTime? ScheduledDateTime { get; set; }
        public string? RecurrencePattern { get; set; } // JSON
        public bool TimeZoneAware { get; set; }
        public string? TimeZone { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}
