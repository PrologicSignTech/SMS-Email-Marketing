using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Pricing;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IRepository<PricingModel> _pricingModelRepository;
        private readonly IRepository<ChannelPricing> _channelPricingRepository;
        private readonly IRepository<RegionPricing> _regionPricingRepository;
        private readonly IRepository<UsagePricing> _usagePricingRepository;
        private readonly IRepository<TaxConfiguration> _taxConfigurationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PricingService> _logger;

        public PricingService(
            IRepository<PricingModel> pricingModelRepository,
            IRepository<ChannelPricing> channelPricingRepository,
            IRepository<RegionPricing> regionPricingRepository,
            IRepository<UsagePricing> usagePricingRepository,
            IRepository<TaxConfiguration> taxConfigurationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PricingService> logger)
        {
            _pricingModelRepository = pricingModelRepository;
            _channelPricingRepository = channelPricingRepository;
            _regionPricingRepository = regionPricingRepository;
            _usagePricingRepository = usagePricingRepository;
            _taxConfigurationRepository = taxConfigurationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Pricing Model Management (12.5.1)
        public async Task<PricingModelDto> CreatePricingModelAsync(CreatePricingModelDto dto)
        {
            var pricingModel = _mapper.Map<PricingModel>(dto);
            pricingModel.IsActive = true;

            await _pricingModelRepository.AddAsync(pricingModel);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pricing model {ModelName} created successfully", pricingModel.Name);

            return await GetPricingModelByIdAsync(pricingModel.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created pricing model");
        }

        public async Task<PricingModelDto?> GetPricingModelByIdAsync(int modelId)
        {
            var model = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == modelId && !m.IsDeleted);

            if (model == null)
                return null;

            var dto = _mapper.Map<PricingModelDto>(model);

            // Load related pricing configurations
            dto.ChannelPricings = await GetChannelPricingsByModelAsync(modelId);
            dto.RegionPricings = await GetRegionPricingsByModelAsync(modelId);
            dto.UsagePricings = await GetUsagePricingsByModelAsync(modelId);

            return dto;
        }

        public async Task<PaginatedResult<PricingModelDto>> GetPricingModelsAsync(PagedRequest request)
        {
            var query = (await _pricingModelRepository.FindAsync(m => !m.IsDeleted)).AsQueryable();

            // Apply sorting by priority and creation date
            query = query.OrderByDescending(m => m.Priority).ThenByDescending(m => m.CreatedAt);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var models = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<PricingModelDto>();
            foreach (var model in models)
            {
                var dto = await GetPricingModelByIdAsync(model.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return new PaginatedResult<PricingModelDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<bool> UpdatePricingModelAsync(int modelId, UpdatePricingModelDto dto)
        {
            var model = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == modelId && !m.IsDeleted);

            if (model == null)
                return false;

            _mapper.Map(dto, model);
            model.UpdatedAt = DateTime.UtcNow;

            _pricingModelRepository.Update(model);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pricing model {ModelId} updated successfully", modelId);
            return true;
        }

        public async Task<bool> DeletePricingModelAsync(int modelId)
        {
            var model = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == modelId && !m.IsDeleted);

            if (model == null)
                return false;

            model.IsDeleted = true;
            model.UpdatedAt = DateTime.UtcNow;

            _pricingModelRepository.Update(model);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pricing model {ModelId} deleted successfully", modelId);
            return true;
        }

        // Channel-Based Pricing (12.5.2)
        public async Task<ChannelPricingDto> CreateChannelPricingAsync(CreateChannelPricingDto dto)
        {
            // Verify pricing model exists
            var pricingModel = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == dto.PricingModelId && !m.IsDeleted);

            if (pricingModel == null)
            {
                _logger.LogWarning("Pricing model {ModelId} not found", dto.PricingModelId);
                throw new InvalidOperationException($"Pricing model with ID {dto.PricingModelId} not found");
            }

            var channelPricing = _mapper.Map<ChannelPricing>(dto);
            channelPricing.IsActive = true;

            await _channelPricingRepository.AddAsync(channelPricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Channel pricing created for model {ModelId} and channel {Channel}", 
                dto.PricingModelId, dto.Channel);

            return await GetChannelPricingByIdAsync(channelPricing.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created channel pricing");
        }

        public async Task<ChannelPricingDto?> GetChannelPricingByIdAsync(int pricingId)
        {
            var pricing = await _channelPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return null;

            var dto = _mapper.Map<ChannelPricingDto>(pricing);

            var model = await _pricingModelRepository.GetByIdAsync(pricing.PricingModelId);
            dto.PricingModelName = model?.Name;

            return dto;
        }

        public async Task<List<ChannelPricingDto>> GetChannelPricingsByModelAsync(int modelId)
        {
            var pricings = await _channelPricingRepository.FindAsync(p =>
                p.PricingModelId == modelId && !p.IsDeleted);

            var dtos = new List<ChannelPricingDto>();
            foreach (var pricing in pricings.OrderBy(p => p.Channel))
            {
                var dto = await GetChannelPricingByIdAsync(pricing.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<bool> UpdateChannelPricingAsync(int pricingId, UpdateChannelPricingDto dto)
        {
            var pricing = await _channelPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            _mapper.Map(dto, pricing);
            pricing.UpdatedAt = DateTime.UtcNow;

            _channelPricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Channel pricing {PricingId} updated successfully", pricingId);
            return true;
        }

        public async Task<bool> DeleteChannelPricingAsync(int pricingId)
        {
            var pricing = await _channelPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            pricing.IsDeleted = true;
            pricing.UpdatedAt = DateTime.UtcNow;

            _channelPricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Channel pricing {PricingId} deleted successfully", pricingId);
            return true;
        }

        // Region-Based Pricing (12.5.3)
        public async Task<RegionPricingDto> CreateRegionPricingAsync(CreateRegionPricingDto dto)
        {
            // Verify pricing model exists
            var pricingModel = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == dto.PricingModelId && !m.IsDeleted);

            if (pricingModel == null)
            {
                _logger.LogWarning("Pricing model {ModelId} not found", dto.PricingModelId);
                throw new InvalidOperationException($"Pricing model with ID {dto.PricingModelId} not found");
            }

            var regionPricing = _mapper.Map<RegionPricing>(dto);
            regionPricing.IsActive = true;

            await _regionPricingRepository.AddAsync(regionPricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Region pricing created for model {ModelId} and region {RegionCode}", 
                dto.PricingModelId, dto.RegionCode);

            return await GetRegionPricingByIdAsync(regionPricing.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created region pricing");
        }

        public async Task<RegionPricingDto?> GetRegionPricingByIdAsync(int pricingId)
        {
            var pricing = await _regionPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return null;

            var dto = _mapper.Map<RegionPricingDto>(pricing);

            var model = await _pricingModelRepository.GetByIdAsync(pricing.PricingModelId);
            dto.PricingModelName = model?.Name;

            return dto;
        }

        public async Task<List<RegionPricingDto>> GetRegionPricingsByModelAsync(int modelId)
        {
            var pricings = await _regionPricingRepository.FindAsync(p =>
                p.PricingModelId == modelId && !p.IsDeleted);

            var dtos = new List<RegionPricingDto>();
            foreach (var pricing in pricings.OrderBy(p => p.RegionName))
            {
                var dto = await GetRegionPricingByIdAsync(pricing.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<bool> UpdateRegionPricingAsync(int pricingId, UpdateRegionPricingDto dto)
        {
            var pricing = await _regionPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            _mapper.Map(dto, pricing);
            pricing.UpdatedAt = DateTime.UtcNow;

            _regionPricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Region pricing {PricingId} updated successfully", pricingId);
            return true;
        }

        public async Task<bool> DeleteRegionPricingAsync(int pricingId)
        {
            var pricing = await _regionPricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            pricing.IsDeleted = true;
            pricing.UpdatedAt = DateTime.UtcNow;

            _regionPricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Region pricing {PricingId} deleted successfully", pricingId);
            return true;
        }

        // Usage-Based Pricing (12.5.4)
        public async Task<UsagePricingDto> CreateUsagePricingAsync(CreateUsagePricingDto dto)
        {
            // Verify pricing model exists
            var pricingModel = await _pricingModelRepository.FirstOrDefaultAsync(m =>
                m.Id == dto.PricingModelId && !m.IsDeleted);

            if (pricingModel == null)
            {
                _logger.LogWarning("Pricing model {ModelId} not found", dto.PricingModelId);
                throw new InvalidOperationException($"Pricing model with ID {dto.PricingModelId} not found");
            }

            var usagePricing = _mapper.Map<UsagePricing>(dto);
            usagePricing.IsActive = true;

            await _usagePricingRepository.AddAsync(usagePricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Usage pricing created for model {ModelId}, type {Type}, tier {TierStart}-{TierEnd}", 
                dto.PricingModelId, dto.Type, dto.TierStart, dto.TierEnd);

            return await GetUsagePricingByIdAsync(usagePricing.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created usage pricing");
        }

        public async Task<UsagePricingDto?> GetUsagePricingByIdAsync(int pricingId)
        {
            var pricing = await _usagePricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return null;

            var dto = _mapper.Map<UsagePricingDto>(pricing);

            var model = await _pricingModelRepository.GetByIdAsync(pricing.PricingModelId);
            dto.PricingModelName = model?.Name;

            return dto;
        }

        public async Task<List<UsagePricingDto>> GetUsagePricingsByModelAsync(int modelId)
        {
            var pricings = await _usagePricingRepository.FindAsync(p =>
                p.PricingModelId == modelId && !p.IsDeleted);

            var dtos = new List<UsagePricingDto>();
            foreach (var pricing in pricings.OrderBy(p => p.Type).ThenBy(p => p.TierStart))
            {
                var dto = await GetUsagePricingByIdAsync(pricing.Id);
                if (dto != null)
                    dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<bool> UpdateUsagePricingAsync(int pricingId, UpdateUsagePricingDto dto)
        {
            var pricing = await _usagePricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            _mapper.Map(dto, pricing);
            pricing.UpdatedAt = DateTime.UtcNow;

            _usagePricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Usage pricing {PricingId} updated successfully", pricingId);
            return true;
        }

        public async Task<bool> DeleteUsagePricingAsync(int pricingId)
        {
            var pricing = await _usagePricingRepository.FirstOrDefaultAsync(p =>
                p.Id == pricingId && !p.IsDeleted);

            if (pricing == null)
                return false;

            pricing.IsDeleted = true;
            pricing.UpdatedAt = DateTime.UtcNow;

            _usagePricingRepository.Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Usage pricing {PricingId} deleted successfully", pricingId);
            return true;
        }

        // Tax & Fee Configuration (12.5.5)
        public async Task<TaxConfigurationDto> CreateTaxConfigurationAsync(CreateTaxConfigurationDto dto)
        {
            var taxConfig = _mapper.Map<TaxConfiguration>(dto);
            taxConfig.IsActive = true;

            await _taxConfigurationRepository.AddAsync(taxConfig);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tax configuration {ConfigName} created successfully", taxConfig.Name);

            return _mapper.Map<TaxConfigurationDto>(taxConfig);
        }

        public async Task<TaxConfigurationDto?> GetTaxConfigurationByIdAsync(int configId)
        {
            var config = await _taxConfigurationRepository.FirstOrDefaultAsync(c =>
                c.Id == configId && !c.IsDeleted);

            if (config == null)
                return null;

            return _mapper.Map<TaxConfigurationDto>(config);
        }

        public async Task<PaginatedResult<TaxConfigurationDto>> GetTaxConfigurationsAsync(PagedRequest request)
        {
            var query = (await _taxConfigurationRepository.FindAsync(c => !c.IsDeleted)).AsQueryable();

            // Apply sorting by priority
            query = query.OrderByDescending(c => c.Priority).ThenBy(c => c.Name);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var configs = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = configs.Select(c => _mapper.Map<TaxConfigurationDto>(c)).ToList();

            return new PaginatedResult<TaxConfigurationDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<bool> UpdateTaxConfigurationAsync(int configId, UpdateTaxConfigurationDto dto)
        {
            var config = await _taxConfigurationRepository.FirstOrDefaultAsync(c =>
                c.Id == configId && !c.IsDeleted);

            if (config == null)
                return false;

            _mapper.Map(dto, config);
            config.UpdatedAt = DateTime.UtcNow;

            _taxConfigurationRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tax configuration {ConfigId} updated successfully", configId);
            return true;
        }

        public async Task<bool> DeleteTaxConfigurationAsync(int configId)
        {
            var config = await _taxConfigurationRepository.FirstOrDefaultAsync(c =>
                c.Id == configId && !c.IsDeleted);

            if (config == null)
                return false;

            config.IsDeleted = true;
            config.UpdatedAt = DateTime.UtcNow;

            _taxConfigurationRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tax configuration {ConfigId} deleted successfully", configId);
            return true;
        }
    }
}
