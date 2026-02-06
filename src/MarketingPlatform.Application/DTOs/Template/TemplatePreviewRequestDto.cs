namespace MarketingPlatform.Application.DTOs.Template
{
    public class TemplatePreviewRequestDto
    {
        public int TemplateId { get; set; }
        public Dictionary<string, string> VariableValues { get; set; } = new();
        public int? ContactId { get; set; }
    }
}
