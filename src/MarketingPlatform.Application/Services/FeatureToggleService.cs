using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class FeatureToggleService : IFeatureToggleService
    {
        private readonly IRepository<FeatureToggle> _featureToggleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FeatureToggleService> _logger;

        public FeatureToggleService(
            IRepository<FeatureToggle> featureToggleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<FeatureToggleService> logger)
        {
            _featureToggleRepository = featureToggleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FeatureToggleDto?> GetFeatureToggleByIdAsync(int id)
        {
            var feature = await _featureToggleRepository.GetByIdAsync(id);
            return feature != null ? _mapper.Map<FeatureToggleDto>(feature) : null;
        }

        public async Task<FeatureToggleDto?> GetFeatureToggleByNameAsync(string name)
        {
            var feature = await _featureToggleRepository.FirstOrDefaultAsync(f => f.Name == name && !f.IsDeleted);
            return feature != null ? _mapper.Map<FeatureToggleDto>(feature) : null;
        }

        public async Task<PaginatedResult<FeatureToggleDto>> GetFeatureTogglesAsync(PagedRequest request)
        {
            var query = _featureToggleRepository.GetQueryable().Where(f => !f.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(f =>
                    f.Name.Contains(request.SearchTerm) ||
                    f.DisplayName.Contains(request.SearchTerm) ||
                    (f.Description != null && f.Description.Contains(request.SearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var features = await query
                .OrderBy(f => f.Category)
                .ThenBy(f => f.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var featureDtos = _mapper.Map<List<FeatureToggleDto>>(features);

            return new PaginatedResult<FeatureToggleDto>
            {
                Items = featureDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<FeatureToggleDto>> GetFeatureTogglesByCategoryAsync(string category)
        {
            var features = await _featureToggleRepository.FindAsync(f => 
                f.Category == category && !f.IsDeleted);
            return _mapper.Map<List<FeatureToggleDto>>(features);
        }

        public async Task<FeatureToggleDto> CreateFeatureToggleAsync(CreateFeatureToggleDto dto, string userId)
        {
            // Check if feature already exists
            var existingFeature = await _featureToggleRepository.FirstOrDefaultAsync(f => f.Name == dto.Name);
            if (existingFeature != null)
            {
                throw new InvalidOperationException($"Feature toggle with name '{dto.Name}' already exists.");
            }

            var feature = _mapper.Map<FeatureToggle>(dto);
            feature.Status = dto.IsEnabled ? FeatureToggleStatus.Enabled : FeatureToggleStatus.Disabled;
            feature.ModifiedBy = userId;
            feature.CreatedAt = DateTime.UtcNow;

            await _featureToggleRepository.AddAsync(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Feature toggle created: {Name} by user {UserId}", feature.Name, userId);

            return _mapper.Map<FeatureToggleDto>(feature);
        }

        public async Task<bool> UpdateFeatureToggleAsync(int id, UpdateFeatureToggleDto dto, string userId)
        {
            var feature = await _featureToggleRepository.GetByIdAsync(id);
            if (feature == null || feature.IsDeleted)
            {
                return false;
            }

            if (dto.DisplayName != null) feature.DisplayName = dto.DisplayName;
            if (dto.Description != null) feature.Description = dto.Description;
            if (dto.Category != null) feature.Category = dto.Category;
            if (dto.IsEnabled.HasValue)
            {
                feature.IsEnabled = dto.IsEnabled.Value;
                feature.Status = dto.IsEnabled.Value ? FeatureToggleStatus.Enabled : FeatureToggleStatus.Disabled;
            }
            if (dto.EnableAfter.HasValue) feature.EnableAfter = dto.EnableAfter;
            if (dto.DisableAfter.HasValue) feature.DisableAfter = dto.DisableAfter;

            feature.ModifiedBy = userId;
            feature.UpdatedAt = DateTime.UtcNow;

            _featureToggleRepository.Update(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Feature toggle updated: {Name} by user {UserId}", feature.Name, userId);

            return true;
        }

        public async Task<bool> DeleteFeatureToggleAsync(int id)
        {
            var feature = await _featureToggleRepository.GetByIdAsync(id);
            if (feature == null || feature.IsDeleted)
            {
                return false;
            }

            feature.IsDeleted = true;
            feature.UpdatedAt = DateTime.UtcNow;

            _featureToggleRepository.Update(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Feature toggle deleted: {Name}", feature.Name);

            return true;
        }

        public async Task<bool> ToggleFeatureAsync(int id, ToggleFeatureDto dto, string userId)
        {
            var feature = await _featureToggleRepository.GetByIdAsync(id);
            if (feature == null || feature.IsDeleted)
            {
                return false;
            }

            feature.IsEnabled = dto.IsEnabled;
            
            // Determine status based on roles and users
            if (!dto.IsEnabled)
            {
                feature.Status = FeatureToggleStatus.Disabled;
            }
            else if (!string.IsNullOrWhiteSpace(dto.EnabledForRoles))
            {
                feature.Status = FeatureToggleStatus.EnabledForRoles;
                feature.EnabledForRoles = dto.EnabledForRoles;
            }
            else if (!string.IsNullOrWhiteSpace(dto.EnabledForUsers))
            {
                feature.Status = FeatureToggleStatus.EnabledForUsers;
                feature.EnabledForUsers = dto.EnabledForUsers;
            }
            else
            {
                feature.Status = FeatureToggleStatus.Enabled;
            }

            feature.ModifiedBy = userId;
            feature.UpdatedAt = DateTime.UtcNow;

            _featureToggleRepository.Update(feature);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Feature toggle changed: {Name} - Enabled: {IsEnabled} by user {UserId}", 
                feature.Name, dto.IsEnabled, userId);

            return true;
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureName)
        {
            var feature = await _featureToggleRepository.FirstOrDefaultAsync(f => 
                f.Name == featureName && !f.IsDeleted);

            if (feature == null)
            {
                return false;
            }

            // Check time-based enabling/disabling
            var now = DateTime.UtcNow;
            if (feature.EnableAfter.HasValue && now < feature.EnableAfter.Value)
            {
                return false;
            }
            if (feature.DisableAfter.HasValue && now > feature.DisableAfter.Value)
            {
                return false;
            }

            return feature.IsEnabled && feature.Status == FeatureToggleStatus.Enabled;
        }

        public async Task<bool> IsFeatureEnabledForUserAsync(string featureName, string userId)
        {
            var feature = await _featureToggleRepository.FirstOrDefaultAsync(f => 
                f.Name == featureName && !f.IsDeleted);

            if (feature == null || !feature.IsEnabled)
            {
                return false;
            }

            // Check time-based enabling/disabling
            var now = DateTime.UtcNow;
            if (feature.EnableAfter.HasValue && now < feature.EnableAfter.Value)
            {
                return false;
            }
            if (feature.DisableAfter.HasValue && now > feature.DisableAfter.Value)
            {
                return false;
            }

            // Check status
            if (feature.Status == FeatureToggleStatus.Enabled)
            {
                return true;
            }

            if (feature.Status == FeatureToggleStatus.EnabledForUsers)
            {
                var enabledUserIds = feature.EnabledForUsers?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                return enabledUserIds.Contains(userId);
            }

            return false;
        }

        public async Task<bool> IsFeatureEnabledForRoleAsync(string featureName, string roleName)
        {
            var feature = await _featureToggleRepository.FirstOrDefaultAsync(f => 
                f.Name == featureName && !f.IsDeleted);

            if (feature == null || !feature.IsEnabled)
            {
                return false;
            }

            // Check time-based enabling/disabling
            var now = DateTime.UtcNow;
            if (feature.EnableAfter.HasValue && now < feature.EnableAfter.Value)
            {
                return false;
            }
            if (feature.DisableAfter.HasValue && now > feature.DisableAfter.Value)
            {
                return false;
            }

            // Check status
            if (feature.Status == FeatureToggleStatus.Enabled)
            {
                return true;
            }

            if (feature.Status == FeatureToggleStatus.EnabledForRoles)
            {
                var enabledRoles = feature.EnabledForRoles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                return enabledRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
