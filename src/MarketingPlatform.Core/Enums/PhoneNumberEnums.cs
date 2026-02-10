namespace MarketingPlatform.Core.Enums
{
    public enum PhoneNumberType
    {
        Local,
        TollFree,
        ShortCode,
        Mobile
    }

    public enum PhoneNumberCapability
    {
        SMS,
        MMS,
        Both
    }

    public enum PhoneNumberStatus
    {
        Available,
        Active,
        Suspended,
        Released
    }
}
