namespace MarketingPlatform.Application.DTOs.Message
{
    /// <summary>
    /// Result DTO for test send operation
    /// </summary>
    public class TestSendResultDto
    {
        /// <summary>
        /// Number of test messages sent successfully
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of test messages that failed
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// List of recipients and their send status
        /// </summary>
        public List<TestSendRecipientResultDto> Recipients { get; set; } = new();

        /// <summary>
        /// Overall success status
        /// </summary>
        public bool IsSuccess => FailureCount == 0;
    }

    /// <summary>
    /// Individual recipient result for test send
    /// </summary>
    public class TestSendRecipientResultDto
    {
        /// <summary>
        /// Recipient address (email or phone number)
        /// </summary>
        public string Recipient { get; set; } = string.Empty;

        /// <summary>
        /// Whether the send was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if send failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// External message ID from provider
        /// </summary>
        public string? ExternalMessageId { get; set; }
    }
}
