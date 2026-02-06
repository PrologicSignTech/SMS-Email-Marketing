using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Message
{
    /// <summary>
    /// Response DTO for message preview
    /// </summary>
    public class MessagePreviewDto
    {
        /// <summary>
        /// Channel type
        /// </summary>
        public ChannelType Channel { get; set; }

        /// <summary>
        /// Rendered subject line
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Rendered message body
        /// </summary>
        public string MessageBody { get; set; } = string.Empty;

        /// <summary>
        /// Rendered HTML content
        /// </summary>
        public string? HTMLContent { get; set; }

        /// <summary>
        /// Media URLs
        /// </summary>
        public List<string>? MediaUrls { get; set; }

        /// <summary>
        /// Device-specific previews
        /// </summary>
        public List<DevicePreviewDto> DevicePreviews { get; set; } = new();

        /// <summary>
        /// Variables that were missing from the provided values
        /// </summary>
        public List<string> MissingVariables { get; set; } = new();

        /// <summary>
        /// Character count for the message
        /// </summary>
        public int CharacterCount { get; set; }

        /// <summary>
        /// Number of SMS segments (for SMS/MMS)
        /// </summary>
        public int? SmsSegments { get; set; }

        /// <summary>
        /// Validation warnings
        /// </summary>
        public List<string> ValidationWarnings { get; set; } = new();

        /// <summary>
        /// Validation errors
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();
    }
}
