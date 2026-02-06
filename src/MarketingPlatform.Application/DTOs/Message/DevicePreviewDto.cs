namespace MarketingPlatform.Application.DTOs.Message
{
    /// <summary>
    /// Represents a preview of message content for a specific device type
    /// </summary>
    public class DevicePreviewDto
    {
        /// <summary>
        /// Device type (e.g., Desktop, Mobile, Tablet)
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Rendered subject line for this device
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Rendered message body for this device
        /// </summary>
        public string MessageBody { get; set; } = string.Empty;

        /// <summary>
        /// Rendered HTML content for this device
        /// </summary>
        public string? HTMLContent { get; set; }

        /// <summary>
        /// Character count information
        /// </summary>
        public int CharacterCount { get; set; }

        /// <summary>
        /// Indicates if content will be truncated on this device
        /// </summary>
        public bool IsTruncated { get; set; }

        /// <summary>
        /// Warnings or issues specific to this device rendering
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
