using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    /// <summary>
    /// Request DTO for sending test messages
    /// </summary>
    public class TestSendRequestDto
    {
        /// <summary>
        /// Channel type for the test message (SMS, MMS, Email)
        /// </summary>
        public ChannelType Channel { get; set; }

        /// <summary>
        /// Optional campaign ID for context
        /// </summary>
        public int? CampaignId { get; set; }

        /// <summary>
        /// Optional template ID to use for content
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// Test recipient addresses (email addresses or phone numbers)
        /// </summary>
        public List<string> Recipients { get; set; } = new();

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
