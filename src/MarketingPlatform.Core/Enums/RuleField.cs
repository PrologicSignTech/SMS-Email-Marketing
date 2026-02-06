namespace MarketingPlatform.Core.Enums
{
    public enum RuleField
    {
        // Contact basic fields
        Email,
        FirstName,
        LastName,
        PhoneNumber,
        Country,
        City,
        PostalCode,
        IsActive,
        
        // Tag-based rules
        HasTag,
        
        // Custom attributes (requires AttributeName to be specified)
        CustomAttribute,
        
        // Engagement-based rules
        TotalMessagesSent,
        TotalMessagesDelivered,
        TotalClicks,
        EngagementScore,
        LastEngagementDate
    }
}
