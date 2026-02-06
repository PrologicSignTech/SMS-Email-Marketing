namespace MarketingPlatform.Application.DTOs.Template
{
    /// <summary>
    /// Character count information for message content
    /// </summary>
    public class CharacterCountDto
    {
        /// <summary>
        /// Total number of characters in the content
        /// </summary>
        public int CharacterCount { get; set; }

        /// <summary>
        /// Number of SMS segments (160 chars per segment for standard SMS, 70 for Unicode)
        /// Only applicable for SMS/MMS channels
        /// </summary>
        public int? SmsSegments { get; set; }

        /// <summary>
        /// Whether the message contains Unicode characters
        /// </summary>
        public bool ContainsUnicode { get; set; }

        /// <summary>
        /// Maximum recommended character count for the channel
        /// SMS: 160 (or 70 for Unicode), Email Subject: 50-60 chars
        /// </summary>
        public int? RecommendedMaxLength { get; set; }

        /// <summary>
        /// Whether the content exceeds recommended length
        /// </summary>
        public bool ExceedsRecommendedLength { get; set; }
    }
}
