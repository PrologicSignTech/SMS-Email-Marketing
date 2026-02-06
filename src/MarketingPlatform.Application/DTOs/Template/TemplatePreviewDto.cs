namespace MarketingPlatform.Application.DTOs.Template
{
    public class TemplatePreviewDto
    {
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public List<string>? MediaUrls { get; set; }
        public List<string> MissingVariables { get; set; } = new();
        
        /// <summary>
        /// Character count for the subject line (Email only)
        /// </summary>
        public CharacterCountDto? SubjectCharacterCount { get; set; }
        
        /// <summary>
        /// Character count for the message body
        /// </summary>
        public CharacterCountDto? MessageBodyCharacterCount { get; set; }
    }
}
