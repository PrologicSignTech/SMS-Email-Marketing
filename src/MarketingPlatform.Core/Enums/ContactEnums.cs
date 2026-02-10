namespace MarketingPlatform.Core.Enums
{
    public enum SuppressionType
    {
        OptOut,
        Bounce,
        Complaint,
        Manual
    }

    /// <summary>
    /// Events that trigger auto-suppression
    /// </summary>
    public enum SuppressionTrigger
    {
        Unsubscribe,       // User clicks unsubscribe link
        HardBounce,        // Email hard bounce
        SoftBounce,        // Email soft bounce (after threshold)
        SpamComplaint,     // Spam complaint received
        SmsOptOut,         // SMS STOP keyword received
        WhatsAppOptOut,    // WhatsApp opt-out
        InvalidEmail,      // Invalid email detected
        InvalidPhone,      // Invalid phone number detected
        ManualUpload,      // Manual CSV/list upload
        InactivityTimeout  // No engagement for X days
    }

    /// <summary>
    /// Scope of suppression
    /// </summary>
    public enum SuppressionScope
    {
        Global,            // Suppress across all channels
        ChannelSpecific    // Suppress only on specific channel
    }

    /// <summary>
    /// Channel for channel-specific suppression
    /// </summary>
    public enum SuppressionChannel
    {
        All,
        Email,
        SMS,
        MMS,
        WhatsApp
    }
}
