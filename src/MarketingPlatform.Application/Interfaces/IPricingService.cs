using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Pricing;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPricingService
    {
        // Pricing Model Management (12.5.1)
        Task<PricingModelDto> CreatePricingModelAsync(CreatePricingModelDto dto);
        Task<PricingModelDto?> GetPricingModelByIdAsync(int modelId);
        Task<PaginatedResult<PricingModelDto>> GetPricingModelsAsync(PagedRequest request);
        Task<bool> UpdatePricingModelAsync(int modelId, UpdatePricingModelDto dto);
        Task<bool> DeletePricingModelAsync(int modelId);

        // Channel-Based Pricing (12.5.2)
        Task<ChannelPricingDto> CreateChannelPricingAsync(CreateChannelPricingDto dto);
        Task<ChannelPricingDto?> GetChannelPricingByIdAsync(int pricingId);
        Task<List<ChannelPricingDto>> GetChannelPricingsByModelAsync(int modelId);
        Task<bool> UpdateChannelPricingAsync(int pricingId, UpdateChannelPricingDto dto);
        Task<bool> DeleteChannelPricingAsync(int pricingId);

        // Region-Based Pricing (12.5.3)
        Task<RegionPricingDto> CreateRegionPricingAsync(CreateRegionPricingDto dto);
        Task<RegionPricingDto?> GetRegionPricingByIdAsync(int pricingId);
        Task<List<RegionPricingDto>> GetRegionPricingsByModelAsync(int modelId);
        Task<bool> UpdateRegionPricingAsync(int pricingId, UpdateRegionPricingDto dto);
        Task<bool> DeleteRegionPricingAsync(int pricingId);

        // Usage-Based Pricing (12.5.4)
        Task<UsagePricingDto> CreateUsagePricingAsync(CreateUsagePricingDto dto);
        Task<UsagePricingDto?> GetUsagePricingByIdAsync(int pricingId);
        Task<List<UsagePricingDto>> GetUsagePricingsByModelAsync(int modelId);
        Task<bool> UpdateUsagePricingAsync(int pricingId, UpdateUsagePricingDto dto);
        Task<bool> DeleteUsagePricingAsync(int pricingId);

        // Tax & Fee Configuration (12.5.5)
        Task<TaxConfigurationDto> CreateTaxConfigurationAsync(CreateTaxConfigurationDto dto);
        Task<TaxConfigurationDto?> GetTaxConfigurationByIdAsync(int configId);
        Task<PaginatedResult<TaxConfigurationDto>> GetTaxConfigurationsAsync(PagedRequest request);
        Task<bool> UpdateTaxConfigurationAsync(int configId, UpdateTaxConfigurationDto dto);
        Task<bool> DeleteTaxConfigurationAsync(int configId);
    }
}
