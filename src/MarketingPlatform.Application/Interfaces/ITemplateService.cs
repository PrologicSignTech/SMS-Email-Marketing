using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Template;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ITemplateService
    {
        Task<TemplateDto?> GetTemplateByIdAsync(string userId, int templateId);
        Task<PaginatedResult<TemplateDto>> GetTemplatesAsync(string userId, PagedRequest request);
        Task<List<TemplateDto>> GetTemplatesByChannelAsync(string userId, ChannelType channel);
        Task<List<TemplateDto>> GetTemplatesByCategoryAsync(string userId, TemplateCategory category);
        Task<TemplateDto> CreateTemplateAsync(string userId, CreateTemplateDto dto);
        Task<bool> UpdateTemplateAsync(string userId, int templateId, UpdateTemplateDto dto);
        Task<bool> DeleteTemplateAsync(string userId, int templateId);
        Task<bool> DuplicateTemplateAsync(string userId, int templateId);
        Task<bool> SetDefaultTemplateAsync(string userId, int templateId);
        Task<bool> ActivateTemplateAsync(string userId, int templateId);
        Task<bool> DeactivateTemplateAsync(string userId, int templateId);
        Task<TemplatePreviewDto> PreviewTemplateAsync(string userId, TemplatePreviewRequestDto request);
        Task<string> RenderTemplateAsync(int templateId, Dictionary<string, string> variables);
        Task<List<string>> ExtractVariablesFromContentAsync(string content);
        Task<TemplateUsageStatsDto> GetTemplateUsageStatsAsync(string userId, int templateId);
        Task<CharacterCountDto> CalculateCharacterCountAsync(string content, ChannelType channel, bool isSubject);
    }
}
