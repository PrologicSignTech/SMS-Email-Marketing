namespace MarketingPlatform.Core.Enums
{
    public enum ChannelType
    {
        SMS,
        MMS,
        Email
    }

    public enum MessageStatus
    {
        Queued,
        Sending,
        Sent,
        Delivered,
        Failed,
        Bounced
    }

    public enum TemplateCategory
    {
        Promotional,
        Transactional,
        Reminder,
        Alert,
        Custom
    }
}
