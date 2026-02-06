namespace MarketingPlatform.Core.Enums
{
    public enum CampaignType
    {
        SMS,
        MMS,
        Email,
        Multi
    }

    public enum CampaignStatus
    {
        Draft,
        Scheduled,
        Running,
        Paused,
        Completed,
        Failed
    }

    public enum ScheduleType
    {
        OneTime,
        Recurring,
        Drip
    }

    public enum TargetType
    {
        All,
        Groups,
        Segments
    }
}
