namespace MarketingPlatform.Core.Enums
{
    public enum PricingModelType
    {
        Flat,
        Tiered,
        Volume,
        PerUnit
    }

    public enum BillingPeriod
    {
        Monthly,
        Quarterly,
        Yearly,
        OneTime
    }

    public enum UsagePricingType
    {
        PerMessage,
        PerContact,
        PerCampaign,
        PerHour,
        PerDay
    }

    public enum TaxType
    {
        SalesTax,
        VAT,
        GST,
        ServiceFee,
        ProcessingFee
    }

    public enum ReservationStatus
    {
        Pending,
        Approved,
        Rejected,
        Active,
        Expired
    }
}
