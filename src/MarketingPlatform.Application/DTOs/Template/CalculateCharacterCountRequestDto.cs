using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Template
{
    /// <summary>
    /// Request DTO for calculating character count
    /// </summary>
    public class CalculateCharacterCountRequestDto
    {
        /// <summary>
        /// Content to calculate character count for
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Channel type (SMS, MMS, Email)
        /// </summary>
        public ChannelType Channel { get; set; }

        /// <summary>
        /// Whether this is a subject line (for email)
        /// </summary>
        public bool IsSubject { get; set; } = false;
    }
}
