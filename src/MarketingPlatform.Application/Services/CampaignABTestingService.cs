using AutoMapper;
using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class CampaignABTestingService : ICampaignABTestingService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IRepository<CampaignVariant> _variantRepository;
        private readonly IRepository<CampaignVariantAnalytics> _variantAnalyticsRepository;
        private readonly IRepository<CampaignMessage> _messageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CampaignABTestingService> _logger;
        private static readonly Random _random = new Random();
        
        // Traffic allocation tolerance constants
        private const decimal MIN_TRAFFIC_ALLOCATION = 99m;
        private const decimal MAX_TRAFFIC_ALLOCATION = 101m;

        public CampaignABTestingService(
            IRepository<Campaign> campaignRepository,
            IRepository<CampaignVariant> variantRepository,
            IRepository<CampaignVariantAnalytics> variantAnalyticsRepository,
            IRepository<CampaignMessage> messageRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CampaignABTestingService> logger)
        {
            _campaignRepository = campaignRepository;
            _variantRepository = variantRepository;
            _variantAnalyticsRepository = variantAnalyticsRepository;
            _messageRepository = messageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CampaignVariantDto> CreateVariantAsync(string userId, int campaignId, CreateCampaignVariantDto dto)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                throw new InvalidOperationException("Campaign not found or unauthorized");

            if (campaign.Status != CampaignStatus.Draft)
                throw new InvalidOperationException("Variants can only be added to campaigns in Draft status");

            // Mark campaign as A/B test if it isn't already
            if (!campaign.IsABTest)
            {
                campaign.IsABTest = true;
                await _unitOfWork.SaveChangesAsync();
            }

            var variant = _mapper.Map<CampaignVariant>(dto);
            variant.CampaignId = campaignId;
            variant.Channel = campaign.Type == CampaignType.Multi ? dto.Channel : 
                              campaign.Type == CampaignType.Email ? ChannelType.Email :
                              campaign.Type == CampaignType.SMS ? ChannelType.SMS : ChannelType.MMS;

            await _variantRepository.AddAsync(variant);
            await _unitOfWork.SaveChangesAsync();

            // Create analytics record for variant
            var analytics = new CampaignVariantAnalytics
            {
                CampaignVariantId = variant.Id
            };
            await _variantAnalyticsRepository.AddAsync(analytics);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created variant {VariantId} for campaign {CampaignId}", variant.Id, campaignId);

            return await GetVariantByIdAsync(userId, campaignId, variant.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created variant");
        }

        public async Task<CampaignVariantDto?> GetVariantByIdAsync(string userId, int campaignId, int variantId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return null;

            var variant = await _variantRepository.GetQueryable()
                .Where(v => v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted)
                .Include(v => v.Analytics)
                .FirstOrDefaultAsync();

            return variant == null ? null : _mapper.Map<CampaignVariantDto>(variant);
        }

        public async Task<List<CampaignVariantDto>> GetCampaignVariantsAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return new List<CampaignVariantDto>();

            var variants = await _variantRepository.GetQueryable()
                .Where(v => v.CampaignId == campaignId && !v.IsDeleted)
                .Include(v => v.Analytics)
                .OrderByDescending(v => v.IsControl)
                .ThenBy(v => v.Name)
                .ToListAsync();

            return _mapper.Map<List<CampaignVariantDto>>(variants);
        }

        public async Task<bool> UpdateVariantAsync(string userId, int campaignId, int variantId, UpdateCampaignVariantDto dto)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            if (campaign.Status != CampaignStatus.Draft)
                return false;

            var variant = await _variantRepository.FirstOrDefaultAsync(v => 
                v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted);

            if (variant == null)
                return false;

            variant.Name = dto.Name;
            variant.Description = dto.Description;
            variant.TrafficPercentage = dto.TrafficPercentage;
            variant.IsActive = dto.IsActive;
            variant.Subject = dto.Subject;
            variant.MessageBody = dto.MessageBody;
            variant.HTMLContent = dto.HTMLContent;
            variant.MediaUrls = dto.MediaUrls;
            variant.PersonalizationTokens = dto.PersonalizationTokens;
            variant.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated variant {VariantId} for campaign {CampaignId}", variantId, campaignId);
            return true;
        }

        public async Task<bool> DeleteVariantAsync(string userId, int campaignId, int variantId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                return false;

            if (campaign.Status != CampaignStatus.Draft)
                return false;

            var variant = await _variantRepository.FirstOrDefaultAsync(v => 
                v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted);

            if (variant == null)
                return false;

            variant.IsDeleted = true;
            variant.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted variant {VariantId} for campaign {CampaignId}", variantId, campaignId);
            return true;
        }

        public async Task<bool> ActivateVariantAsync(string userId, int campaignId, int variantId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null || campaign.Status != CampaignStatus.Draft)
                return false;

            var variant = await _variantRepository.FirstOrDefaultAsync(v => 
                v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted);

            if (variant == null)
                return false;

            variant.IsActive = true;
            variant.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateVariantAsync(string userId, int campaignId, int variantId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null || campaign.Status != CampaignStatus.Draft)
                return false;

            var variant = await _variantRepository.FirstOrDefaultAsync(v => 
                v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted);

            if (variant == null || variant.IsControl)
                return false;

            variant.IsActive = false;
            variant.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<VariantComparisonDto> CompareVariantsAsync(string userId, int campaignId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null || !campaign.IsABTest)
                return new VariantComparisonDto();

            var variants = await GetCampaignVariantsAsync(userId, campaignId);

            CampaignVariantDto? winningVariant = null;
            string? recommendation = null;

            if (variants.Any() && variants.All(v => v.SentCount > 0))
            {
                // Simple winner determination based on click-through rate
                winningVariant = variants
                    .Where(v => v.Analytics != null && v.Analytics.DeliveryRate > 0)
                    .OrderByDescending(v => v.Analytics!.ClickRate)
                    .ThenByDescending(v => v.Analytics.ConversionRate)
                    .FirstOrDefault();

                if (winningVariant != null)
                {
                    var controlVariant = variants.FirstOrDefault(v => v.IsControl);
                    if (controlVariant != null && winningVariant.Id != controlVariant.Id)
                    {
                        var improvement = winningVariant.Analytics!.ClickRate - controlVariant.Analytics!.ClickRate;
                        if (improvement > 0)
                        {
                            recommendation = $"Variant '{winningVariant.Name}' shows {improvement:F2}% improvement in click rate over control. Consider selecting as winner.";
                        }
                        else
                        {
                            recommendation = "Control variant is performing best. No significant improvement from test variants.";
                        }
                    }
                }
            }
            else
            {
                recommendation = "Insufficient data to determine winning variant. Campaign needs to run longer.";
            }

            return new VariantComparisonDto
            {
                Variants = variants,
                WinningVariant = winningVariant,
                RecommendedAction = recommendation
            };
        }

        public async Task<bool> SelectWinningVariantAsync(string userId, int campaignId, int variantId)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c => 
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null || !campaign.IsABTest)
                return false;

            var variant = await _variantRepository.FirstOrDefaultAsync(v => 
                v.Id == variantId && v.CampaignId == campaignId && !v.IsDeleted);

            if (variant == null)
                return false;

            campaign.WinningVariantId = variantId;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Selected variant {VariantId} as winner for campaign {CampaignId}", variantId, campaignId);
            return true;
        }

        public async Task<CampaignVariantDto?> SelectVariantForRecipientAsync(int campaignId)
        {
            var activeVariants = await _variantRepository.GetQueryable()
                .Where(v => v.CampaignId == campaignId && v.IsActive && !v.IsDeleted)
                .ToListAsync();

            if (!activeVariants.Any())
                return null;

            // Validate traffic allocation
            var totalTraffic = activeVariants.Sum(v => v.TrafficPercentage);
            if (totalTraffic <= 0 || totalTraffic > 100)
                return null;

            // Select variant based on traffic percentage
            var randomValue = _random.NextDouble() * 100;
            
            decimal cumulativePercentage = 0;
            foreach (var variant in activeVariants.OrderBy(v => v.Id))
            {
                cumulativePercentage += variant.TrafficPercentage;
                if (randomValue <= (double)cumulativePercentage)
                {
                    return _mapper.Map<CampaignVariantDto>(variant);
                }
            }

            // Fallback to first variant
            return _mapper.Map<CampaignVariantDto>(activeVariants.First());
        }

        public async Task UpdateVariantAnalyticsAsync(int variantId)
        {
            var variant = await _variantRepository.GetQueryable()
                .Where(v => v.Id == variantId && !v.IsDeleted)
                .Include(v => v.Analytics)
                .Include(v => v.Messages)
                .FirstOrDefaultAsync();

            if (variant == null || variant.Analytics == null)
                return;

            var messages = variant.Messages.ToList();
            
            variant.Analytics.TotalSent = messages.Count(m => m.Status == MessageStatus.Sent || m.Status == MessageStatus.Delivered);
            variant.Analytics.TotalDelivered = messages.Count(m => m.Status == MessageStatus.Delivered);
            variant.Analytics.TotalFailed = messages.Count(m => m.Status == MessageStatus.Failed);

            // Calculate rates
            if (variant.Analytics.TotalSent > 0)
            {
                variant.Analytics.DeliveryRate = (decimal)variant.Analytics.TotalDelivered / variant.Analytics.TotalSent * 100;
            }

            if (variant.Analytics.TotalDelivered > 0)
            {
                variant.Analytics.ClickRate = (decimal)variant.Analytics.TotalClicks / variant.Analytics.TotalDelivered * 100;
                variant.Analytics.OptOutRate = (decimal)variant.Analytics.TotalOptOuts / variant.Analytics.TotalDelivered * 100;
                variant.Analytics.OpenRate = (decimal)variant.Analytics.TotalOpens / variant.Analytics.TotalDelivered * 100;
                variant.Analytics.BounceRate = (decimal)variant.Analytics.TotalBounces / variant.Analytics.TotalDelivered * 100;
                variant.Analytics.ConversionRate = (decimal)variant.Analytics.TotalConversions / variant.Analytics.TotalDelivered * 100;
            }

            variant.Analytics.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ValidateVariantTrafficAllocationAsync(int campaignId)
        {
            var variants = await _variantRepository.GetQueryable()
                .Where(v => v.CampaignId == campaignId && v.IsActive && !v.IsDeleted)
                .ToListAsync();

            if (!variants.Any())
                return false;

            var totalTraffic = variants.Sum(v => v.TrafficPercentage);
            
            // Traffic must be between MIN_TRAFFIC_ALLOCATION and MAX_TRAFFIC_ALLOCATION to account for rounding
            return totalTraffic >= MIN_TRAFFIC_ALLOCATION && totalTraffic <= MAX_TRAFFIC_ALLOCATION;
        }
    }
}
