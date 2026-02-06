using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class FooterSettingsService : IFooterSettingsService
    {
        private readonly IRepository<FooterSettings> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FooterSettingsService> _logger;

        public FooterSettingsService(
            IRepository<FooterSettings> repository,
            IUnitOfWork unitOfWork,
            ILogger<FooterSettingsService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<FooterSettings?> GetActiveAsync()
        {
            var settings = await _repository.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
            return settings;
        }

        public async Task<FooterSettings?> GetByIdAsync(int id)
        {
            var settings = await _repository.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return settings;
        }

        public async Task<FooterSettings> CreateAsync(FooterSettings settings)
        {
            settings.CreatedAt = DateTime.UtcNow;

            // Deactivate other settings
            var existingSettings = await _repository.FindAsync(s => s.IsActive && !s.IsDeleted);
            foreach (var existing in existingSettings)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
                _repository.Update(existing);
            }

            await _repository.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created footer settings");

            return settings;
        }

        public async Task<FooterSettings> UpdateAsync(int id, FooterSettings settings)
        {
            var existing = await _repository.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Footer settings not found");

            existing.CompanyName = settings.CompanyName;
            existing.CompanyDescription = settings.CompanyDescription;
            existing.AddressLine1 = settings.AddressLine1;
            existing.AddressLine2 = settings.AddressLine2;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.BusinessHours = settings.BusinessHours;
            existing.MapEmbedUrl = settings.MapEmbedUrl;
            existing.FacebookUrl = settings.FacebookUrl;
            existing.TwitterUrl = settings.TwitterUrl;
            existing.LinkedInUrl = settings.LinkedInUrl;
            existing.InstagramUrl = settings.InstagramUrl;
            existing.YouTubeUrl = settings.YouTubeUrl;
            existing.CopyrightText = settings.CopyrightText;
            existing.ShowNewsletter = settings.ShowNewsletter;
            existing.NewsletterTitle = settings.NewsletterTitle;
            existing.NewsletterDescription = settings.NewsletterDescription;
            existing.ShowMap = settings.ShowMap;
            existing.IsActive = settings.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated footer settings");

            return existing;
        }
    }
}
