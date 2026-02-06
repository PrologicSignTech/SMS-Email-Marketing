namespace MarketingPlatform.Application.DTOs.Template
{
    public class TemplateVariableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
    }
}
