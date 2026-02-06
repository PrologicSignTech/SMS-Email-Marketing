using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class LandingFeatureService : ILandingFeatureService
    {
        private readonly IRepository<LandingFeature> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LandingFeatureService> _logger;

        public LandingFeatureService(
            IRepository<LandingFeature> repository,
            IUnitOfWork unitOfWork,
            ILogger<LandingFeatureService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<LandingFeature>> GetAllActiveAsync()
        {
            var features = await _repository.FindAsync(f => 
                f.IsActive && f.ShowOnLanding && !f.IsDeleted);
            return features.OrderBy(f => f.DisplayOrder);
        }

        public async Task<LandingFeature?> GetByIdAsync(int id)
        {
            var feature = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            return feature;
        }

        public async Task<LandingFeature> CreateAsync(LandingFeature feature)
        {
            feature.CreatedAt = DateTime.UtcNow;
            
            await _repository.AddAsync(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created landing feature: {Title}", feature.Title);

            return feature;
        }

        public async Task<LandingFeature> UpdateAsync(int id, LandingFeature feature)
        {
            var existing = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            
            if (existing == null)
                throw new InvalidOperationException("Feature not found");

            existing.Title = feature.Title;
            existing.ShortDescription = feature.ShortDescription;
            existing.DetailedDescription = feature.DetailedDescription;
            existing.IconClass = feature.IconClass;
            existing.ColorClass = feature.ColorClass;
            existing.DisplayOrder = feature.DisplayOrder;
            existing.IsActive = feature.IsActive;
            existing.ShowOnLanding = feature.ShowOnLanding;
            existing.CallToActionText = feature.CallToActionText;
            existing.CallToActionUrl = feature.CallToActionUrl;
            
            // Update statistics (for detail page hero)
            existing.StatTitle1 = feature.StatTitle1;
            existing.StatValue1 = feature.StatValue1;
            existing.StatTitle2 = feature.StatTitle2;
            existing.StatValue2 = feature.StatValue2;
            existing.StatTitle3 = feature.StatTitle3;
            existing.StatValue3 = feature.StatValue3;
            
            // Update media and contact fields
            existing.HeaderImageUrl = feature.HeaderImageUrl;
            existing.VideoUrl = feature.VideoUrl;
            existing.GalleryImages = feature.GalleryImages;
            existing.ContactName = feature.ContactName;
            existing.ContactEmail = feature.ContactEmail;
            existing.ContactPhone = feature.ContactPhone;
            existing.ContactMessage = feature.ContactMessage;
            
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated landing feature: {Title}", feature.Title);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var feature = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            
            if (feature == null)
                throw new InvalidOperationException("Feature not found");

            feature.IsDeleted = true;
            feature.UpdatedAt = DateTime.UtcNow;

            _repository.Update(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted landing feature: {Title}", feature.Title);

            return true;
        }
    }
}
