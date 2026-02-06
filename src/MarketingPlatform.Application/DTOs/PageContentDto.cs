namespace MarketingPlatform.Application.DTOs
{
    public class PageContentDto
    {
        public int Id { get; set; }
        public string PageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? MetaDescription { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public bool IsPublished { get; set; } = true;
        public string? LastModifiedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PageContentUpdateDto
    {
        public string PageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? MetaDescription { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}
