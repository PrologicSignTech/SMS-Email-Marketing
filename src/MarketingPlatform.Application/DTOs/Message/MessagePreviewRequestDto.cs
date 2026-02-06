using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    /// <summary>
    /// Request DTO for previewing message content
    /// </summary>
    public class MessagePreviewRequestDto
    {
        /// <summary>
        /// Channel type for the message (SMS, MMS, Email)
        /// </summary>
        public ChannelType Channel { get; set; }

        /// <summary>
        /// Optional campaign ID to use for context
        /// </summary>
        public int? CampaignId { get; set; }

        /// <summary>
        /// Optional template ID to use for content
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// Message subject (for Email)
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Message body text
        /// </summary>
        public string MessageBody { get; set; } = string.Empty;

        /// <summary>
        /// HTML content (for Email)
        /// </summary>
        public string? HTMLContent { get; set; }

        /// <summary>
        /// Media URLs (for MMS)
        /// </summary>
        public List<string>? MediaUrls { get; set; }

        /// <summary>
        /// Variable values for template substitution
        /// </summary>
        public Dictionary<string, string> VariableValues { get; set; } = new();

        /// <summary>
        /// Optional contact ID to use for personalization
        /// </summary>
        public int? ContactId { get; set; }
    }
}
