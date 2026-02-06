using System.Text.Json;
using MarketingPlatform.Application.DTOs;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class PageContentService : IPageContentService
    {
        private readonly IPageContentRepository _pageContentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PageContentService> _logger;

        public PageContentService(
            IPageContentRepository pageContentRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ILogger<PageContentService> logger)
        {
            _pageContentRepository = pageContentRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageContentDto?> GetByPageKeyAsync(string pageKey)
        {
            var pageContent = await _pageContentRepository.GetByPageKeyAsync(pageKey);
            if (pageContent == null)
                return null;

            return MapToDto(pageContent);
        }

        public async Task<IEnumerable<PageContentDto>> GetAllAsync()
        {
            var contents = await _pageContentRepository.GetAllAsync();
            return contents.Select(MapToDto);
        }

        public async Task<PageContentDto> SavePageContentAsync(PageContentDto dto, string userId)
        {
            PageContent? pageContent = null;

            if (dto.Id > 0)
            {
                // Update existing
                pageContent = await _pageContentRepository.GetByIdAsync(dto.Id);
                if (pageContent == null)
                    throw new InvalidOperationException($"Page content with ID {dto.Id} not found");
            }
            else
            {
                // Check if page with this key already exists
                pageContent = await _pageContentRepository.GetByPageKeyAsync(dto.PageKey);
                
                if (pageContent == null)
                {
                    // Create new
                    pageContent = new PageContent
                    {
                        PageKey = dto.PageKey,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _pageContentRepository.AddAsync(pageContent);
                }
            }

            // Update properties
            pageContent.Title = dto.Title;
            pageContent.Content = dto.Content;
            pageContent.MetaDescription = dto.MetaDescription;
            pageContent.IsPublished = dto.IsPublished;
            pageContent.LastModifiedBy = userId;
            pageContent.UpdatedAt = DateTime.UtcNow;
            
            // Only update ImageUrls if provided
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                pageContent.ImageUrls = JsonSerializer.Serialize(dto.ImageUrls);
            }

            if (dto.Id > 0)
            {
                _pageContentRepository.Update(pageContent);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(pageContent);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string pageKey)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size must be less than 5MB");

            try
            {
                using var stream = file.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var folder = $"page-content/{pageKey}";
                
                var fileUrl = await _fileStorageService.UploadFileAsync(stream, fileName, file.ContentType, folder);

                // Add to page content's image URLs
                var pageContent = await _pageContentRepository.GetByPageKeyAsync(pageKey);
                if (pageContent != null)
                {
                    var imageUrls = string.IsNullOrEmpty(pageContent.ImageUrls)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(pageContent.ImageUrls) ?? new List<string>();

                    imageUrls.Add(fileUrl);
                    pageContent.ImageUrls = JsonSerializer.Serialize(imageUrls);
                    pageContent.UpdatedAt = DateTime.UtcNow;
                    
                    _pageContentRepository.Update(pageContent);
                    await _unitOfWork.SaveChangesAsync();
                }

                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for page {PageKey}", pageKey);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, string pageKey)
        {
            try
            {
                // Remove from page content's image URLs
                var pageContent = await _pageContentRepository.GetByPageKeyAsync(pageKey);
                if (pageContent != null && !string.IsNullOrEmpty(pageContent.ImageUrls))
                {
                    var imageUrls = JsonSerializer.Deserialize<List<string>>(pageContent.ImageUrls) ?? new List<string>();
                    imageUrls.Remove(imageUrl);
                    pageContent.ImageUrls = JsonSerializer.Serialize(imageUrls);
                    pageContent.UpdatedAt = DateTime.UtcNow;
                    
                    _pageContentRepository.Update(pageContent);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Delete from file storage
                // Extract filename from URL - handles both simple paths and URLs with query strings
                var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                var fileKey = uri.IsAbsoluteUri ? Path.GetFileName(uri.LocalPath) : Path.GetFileName(imageUrl);
                await _fileStorageService.DeleteFileAsync(fileKey);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageUrl} for page {PageKey}", imageUrl, pageKey);
                return false;
            }
        }

        private PageContentDto MapToDto(PageContent entity)
        {
            var imageUrls = string.IsNullOrEmpty(entity.ImageUrls)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.ImageUrls) ?? new List<string>();

            return new PageContentDto
            {
                Id = entity.Id,
                PageKey = entity.PageKey,
                Title = entity.Title,
                Content = entity.Content,
                MetaDescription = entity.MetaDescription,
                ImageUrls = imageUrls,
                IsPublished = entity.IsPublished,
                LastModifiedBy = entity.LastModifiedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
