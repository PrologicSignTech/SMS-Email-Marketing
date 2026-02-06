using AutoMapper;
using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class PlatformConfigurationService : IPlatformConfigurationService
    {
        private readonly IPlatformConfigurationRepository _configRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformConfigurationService> _logger;

        public PlatformConfigurationService(
            IPlatformConfigurationRepository configRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PlatformConfigurationService> logger)
        {
            _configRepository = configRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlatformConfigurationDto?> GetByKeyAsync(string key)
        {
            var config = await _configRepository.GetByKeyAsync(key);
            return config == null ? null : _mapper.Map<PlatformConfigurationDto>(config);
        }

        public async Task<IEnumerable<PlatformConfigurationDto>> GetByCategoryAsync(ConfigurationCategory category)
        {
            var configs = await _configRepository.GetByCategoryAsync(category);
            return _mapper.Map<IEnumerable<PlatformConfigurationDto>>(configs);
        }

        public async Task<IEnumerable<PlatformConfigurationDto>> GetActiveConfigurationsAsync()
        {
            var configs = await _configRepository.GetActiveConfigurationsAsync();
            return _mapper.Map<IEnumerable<PlatformConfigurationDto>>(configs);
        }

        public async Task<bool> UpdateConfigurationAsync(string modifiedBy, UpdateConfigurationDto request)
        {
            _logger.LogInformation("Updating configuration {Key} by {ModifiedBy}", request.Key, modifiedBy);

            var result = await _configRepository.UpdateConfigurationAsync(request.Key, request.Value, modifiedBy);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Configuration {Key} updated successfully", request.Key);
            }
            else
            {
                _logger.LogWarning("Failed to update configuration {Key} - not found", request.Key);
            }

            return result;
        }

        public async Task<bool> ToggleFeatureAsync(string modifiedBy, PlatformFeature feature, bool enabled)
        {
            var key = $"Feature.{feature}";
            _logger.LogInformation("Toggling feature {Feature} to {Enabled} by {ModifiedBy}", feature, enabled, modifiedBy);

            var result = await _configRepository.UpdateConfigurationAsync(key, enabled.ToString(), modifiedBy);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Feature {Feature} toggled to {Enabled}", feature, enabled);
            }
            else
            {
                _logger.LogWarning("Failed to toggle feature {Feature} - configuration not found", feature);
            }

            return result;
        }
    }
}
