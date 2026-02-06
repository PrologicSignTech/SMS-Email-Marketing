using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.DTOs.Template
{
    public class UpdateTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Subject { get; set; }
        public string MessageBody { get; set; } = string.Empty;
        public string? HTMLContent { get; set; }
        public List<string>? DefaultMediaUrls { get; set; }
        public List<TemplateVariableDto>? Variables { get; set; }
    }
}
