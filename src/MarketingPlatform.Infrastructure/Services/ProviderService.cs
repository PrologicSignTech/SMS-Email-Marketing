using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Provider;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IRepository<MessageProvider> _providerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProviderService> _logger;

        public ProviderService(
            IRepository<MessageProvider> providerRepository,
            IUnitOfWork unitOfWork,
            ILogger<ProviderService> logger)
        {
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaginatedResult<ProviderDto>> GetProvidersAsync(PagedRequest request)
        {
            var query = _providerRepository.GetQueryable()
                .Where(p => !p.IsDeleted);

            // Search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Type.ToString().ToLower().Contains(searchLower));
            }

            // Sort
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "type" => request.SortDescending ? query.OrderByDescending(p => p.Type) : query.OrderBy(p => p.Type),
                "isactive" => request.SortDescending ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
                "healthstatus" => request.SortDescending ? query.OrderByDescending(p => p.HealthStatus) : query.OrderBy(p => p.HealthStatus),
                _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProviderDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = p.Type,
                    IsActive = p.IsActive,
                    IsPrimary = p.IsPrimary,
                    HealthStatus = p.HealthStatus,
                    LastHealthCheck = p.LastHealthCheck,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToList();

            return new PaginatedResult<ProviderDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<ProviderDetailDto?> GetProviderByIdAsync(int id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null || provider.IsDeleted)
                return null;

            return new ProviderDetailDto
            {
                Id = provider.Id,
                Name = provider.Name,
                Type = provider.Type,
                ApiKey = MaskApiKey(provider.ApiKey),
                Configuration = provider.Configuration,
                IsActive = provider.IsActive,
                IsPrimary = provider.IsPrimary,
                HealthStatus = provider.HealthStatus,
                LastHealthCheck = provider.LastHealthCheck,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
        }

        public async Task<ProviderDto> CreateProviderAsync(CreateProviderDto dto)
        {
            var provider = new MessageProvider
            {
                Name = dto.Name,
                Type = dto.Type,
                ApiKey = dto.ApiKey,
                ApiSecret = dto.ApiSecret,
                Configuration = dto.Configuration,
                IsActive = dto.IsActive,
                IsPrimary = dto.IsPrimary,
                HealthStatus = HealthStatus.Unknown
            };

            await _providerRepository.AddAsync(provider);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created provider {Name} (Type: {Type})", provider.Name, provider.Type);

            return new ProviderDto
            {
                Id = provider.Id,
                Name = provider.Name,
                Type = provider.Type,
                IsActive = provider.IsActive,
                IsPrimary = provider.IsPrimary,
                HealthStatus = provider.HealthStatus,
                CreatedAt = provider.CreatedAt
            };
        }

        public async Task<bool> UpdateProviderAsync(int id, UpdateProviderDto dto)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null || provider.IsDeleted)
                return false;

            provider.Name = dto.Name;
            provider.Type = dto.Type;
            provider.Configuration = dto.Configuration;
            provider.IsActive = dto.IsActive;
            provider.IsPrimary = dto.IsPrimary;
            provider.UpdatedAt = DateTime.UtcNow;

            // Only update credentials if provided (non-null means the user wants to change them)
            if (dto.ApiKey != null)
                provider.ApiKey = dto.ApiKey;
            if (dto.ApiSecret != null)
                provider.ApiSecret = dto.ApiSecret;

            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated provider {Id} ({Name})", id, provider.Name);
            return true;
        }

        public async Task<bool> DeleteProviderAsync(int id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null || provider.IsDeleted)
                return false;

            provider.IsDeleted = true;
            provider.UpdatedAt = DateTime.UtcNow;
            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted provider {Id} ({Name})", id, provider.Name);
            return true;
        }

        public async Task<bool> TestProviderAsync(int id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null || provider.IsDeleted)
                return false;

            // Mock test - in production, this would actually test the provider connection
            provider.HealthStatus = HealthStatus.Healthy;
            provider.LastHealthCheck = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;
            _providerRepository.Update(provider);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tested provider {Id} ({Name}) - Status: Healthy", id, provider.Name);
            return true;
        }

        public Task<List<string>> GetChannelTypesAsync()
        {
            var channelTypes = Enum.GetNames(typeof(ProviderType)).ToList();
            return Task.FromResult(channelTypes);
        }

        private static string? MaskApiKey(string? apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
                return apiKey != null ? "****" : null;

            return apiKey[..4] + new string('*', apiKey.Length - 8) + apiKey[^4..];
        }
    }
}
