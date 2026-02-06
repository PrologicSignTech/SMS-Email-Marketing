namespace MarketingPlatform.Core.Enums
{
    public enum ConsentChannel
    {
        SMS,
        MMS,
        Email,
        All
    }

    public enum ConsentStatus
    {
        OptedIn,
        OptedOut,
        Pending,
        Unknown
    }

    public enum ConsentSource
    {
        WebForm,
        API,
        Import,
        Keyword,
        Manual,
        Campaign
    }

    public enum ComplianceActionType
    {
        OptIn,
        OptOut,
        ConsentGiven,
        ConsentRevoked,
        SuppressionAdded,
        SuppressionRemoved,
        QuietHoursViolation,
        ComplianceCheck
    }
}
