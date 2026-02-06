using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.URL;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace MarketingPlatform.Application.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private const int ShortCodeLength = 6;
        private const int ShortCodeMinLength = 4;
        private const int ShortCodeMaxLength = 12;
        private const int MaxGenerationAttempts = 10;
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly IRepository<URLShortener> _urlShortenerRepository;
        private readonly IRepository<URLClick> _urlClickRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UrlShortenerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public UrlShortenerService(
            IRepository<URLShortener> urlShortenerRepository,
            IRepository<URLClick> urlClickRepository,
            IRepository<Campaign> campaignRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UrlShortenerService> logger,
            IConfiguration configuration)
        {
            _urlShortenerRepository = urlShortenerRepository;
            _urlClickRepository = urlClickRepository;
            _campaignRepository = campaignRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _baseUrl = _configuration["UrlShortener:BaseUrl"] ?? "https://short.link";
        }

        public async Task<UrlShortenerDto> CreateShortenedUrlAsync(string userId, CreateShortenedUrlDto dto)
        {
            // Verify campaign ownership
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == dto.CampaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                throw new UnauthorizedAccessException("Campaign not found or access denied");

            // Generate or validate short code
            string shortCode;
            if (!string.IsNullOrWhiteSpace(dto.CustomShortCode))
            {
                // Validate custom short code
                if (!IsValidShortCode(dto.CustomShortCode))
                    throw new ArgumentException("Invalid short code. Must be 4-12 alphanumeric characters");

                // Check if already exists
                var existing = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                    u.ShortCode == dto.CustomShortCode && !u.IsDeleted);

                if (existing != null)
                    throw new ArgumentException("Short code already exists");

                shortCode = dto.CustomShortCode;
            }
            else
            {
                // Generate unique short code
                shortCode = await GenerateUniqueShortCodeAsync();
            }

            var urlShortener = new URLShortener
            {
                CampaignId = dto.CampaignId,
                OriginalUrl = dto.OriginalUrl,
                ShortCode = shortCode,
                ShortUrl = $"{_baseUrl}/{shortCode}",
                ClickCount = 0
            };

            await _urlShortenerRepository.AddAsync(urlShortener);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Created shortened URL {urlShortener.ShortUrl} for campaign {dto.CampaignId}");

            return _mapper.Map<UrlShortenerDto>(urlShortener);
        }

        public async Task<UrlShortenerDto?> GetShortenedUrlByIdAsync(string userId, int id)
        {
            var urlShortener = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                u.Id == id && !u.IsDeleted);

            if (urlShortener == null)
                return null;

            // Load campaign to verify ownership
            var campaign = await _campaignRepository.GetByIdAsync(urlShortener.CampaignId);
            if (campaign == null || campaign.UserId != userId)
                return null;

            return _mapper.Map<UrlShortenerDto>(urlShortener);
        }

        public async Task<UrlShortenerDto?> GetShortenedUrlByCodeAsync(string shortCode)
        {
            var urlShortener = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                u.ShortCode == shortCode && !u.IsDeleted);

            if (urlShortener == null)
                return null;

            return _mapper.Map<UrlShortenerDto>(urlShortener);
        }

        public async Task<List<UrlShortenerDto>> GetUrlsByCampaignAsync(string userId, int campaignId)
        {
            // Verify campaign ownership
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                throw new UnauthorizedAccessException("Campaign not found or access denied");

            var urls = await _urlShortenerRepository.FindAsync(u =>
                u.CampaignId == campaignId && !u.IsDeleted);

            return urls
                .OrderByDescending(u => u.ClickCount)
                .ThenByDescending(u => u.CreatedAt)
                .Select(u => _mapper.Map<UrlShortenerDto>(u))
                .ToList();
        }

        public async Task<PaginatedResult<UrlShortenerDto>> GetUrlsAsync(string userId, PagedRequest request)
        {
            // Get all campaigns for this user
            var userCampaigns = await _campaignRepository.FindAsync(c =>
                c.UserId == userId && !c.IsDeleted);
            var campaignIds = userCampaigns.Select(c => c.Id).ToList();

            var query = (await _urlShortenerRepository.FindAsync(u =>
                campaignIds.Contains(u.CampaignId) && !u.IsDeleted)).AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.ShortCode.Contains(request.SearchTerm) ||
                    u.OriginalUrl.Contains(request.SearchTerm));
            }

            // Sort
            query = request.SortBy?.ToLower() switch
            {
                "clicks" => request.SortDescending ? query.OrderByDescending(u => u.ClickCount) : query.OrderBy(u => u.ClickCount),
                "created" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => _mapper.Map<UrlShortenerDto>(u))
                .ToList();

            return new PaginatedResult<UrlShortenerDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<bool> TrackClickAsync(string shortCode, UrlClickDto clickInfo)
        {
            var urlShortener = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                u.ShortCode == shortCode && !u.IsDeleted);

            if (urlShortener == null)
                return false;

            // Create click record
            var click = new URLClick
            {
                URLShortenerId = urlShortener.Id,
                IpAddress = clickInfo.IpAddress,
                UserAgent = clickInfo.UserAgent,
                Referrer = clickInfo.Referrer,
                ClickedAt = DateTime.UtcNow
            };

            await _urlClickRepository.AddAsync(click);

            // Increment click count
            urlShortener.ClickCount++;
            urlShortener.UpdatedAt = DateTime.UtcNow;
            _urlShortenerRepository.Update(urlShortener);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Tracked click for short URL {shortCode}");

            return true;
        }

        public async Task<UrlClickStatsDto> GetClickStatsAsync(string userId, int urlShortenerId)
        {
            var urlShortener = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                u.Id == urlShortenerId && !u.IsDeleted);

            if (urlShortener == null)
                throw new KeyNotFoundException("URL not found");

            // Load campaign to verify ownership
            var campaign = await _campaignRepository.GetByIdAsync(urlShortener.CampaignId);
            if (campaign == null || campaign.UserId != userId)
                throw new UnauthorizedAccessException("URL not found or access denied");

            var clicks = await _urlClickRepository.FindAsync(c =>
                c.URLShortenerId == urlShortenerId && !c.IsDeleted);

            var clickList = clicks.ToList();
            var uniqueClicks = clickList.DistinctBy(c => c.IpAddress).Count();

            // Group clicks by date
            var clicksByDate = clickList
                .GroupBy(c => c.ClickedAt.Date)
                .Select(g => new ClickByDateDto
                {
                    Date = g.Key,
                    ClickCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Top referrers
            var topReferrers = clickList
                .Where(c => !string.IsNullOrWhiteSpace(c.Referrer))
                .GroupBy(c => c.Referrer!)
                .Select(g => new ClickByReferrerDto
                {
                    Referrer = g.Key,
                    ClickCount = g.Count()
                })
                .OrderByDescending(x => x.ClickCount)
                .Take(10)
                .ToList();

            return new UrlClickStatsDto
            {
                UrlShortenerId = urlShortenerId,
                ShortUrl = urlShortener.ShortUrl,
                OriginalUrl = urlShortener.OriginalUrl,
                TotalClicks = urlShortener.ClickCount,
                UniqueClicks = uniqueClicks,
                FirstClickAt = clickList.Any() ? clickList.Min(c => c.ClickedAt) : null,
                LastClickAt = clickList.Any() ? clickList.Max(c => c.ClickedAt) : null,
                ClicksByDate = clicksByDate,
                TopReferrers = topReferrers
            };
        }

        public async Task<CampaignUrlStatsDto> GetCampaignUrlStatsAsync(string userId, int campaignId)
        {
            // Verify campaign ownership
            var campaign = await _campaignRepository.FirstOrDefaultAsync(c =>
                c.Id == campaignId && c.UserId == userId && !c.IsDeleted);

            if (campaign == null)
                throw new UnauthorizedAccessException("Campaign not found or access denied");

            var urls = await _urlShortenerRepository.FindAsync(u =>
                u.CampaignId == campaignId && !u.IsDeleted);

            var urlList = urls.ToList();
            var totalClicks = urlList.Sum(u => u.ClickCount);

            // Get unique clicks across all URLs
            var allUrlIds = urlList.Select(u => u.Id).ToList();
            var allClicks = await _urlClickRepository.FindAsync(c =>
                allUrlIds.Contains(c.URLShortenerId) && !c.IsDeleted);

            var uniqueClicks = allClicks.DistinctBy(c => c.IpAddress).Count();

            var topUrls = urlList
                .OrderByDescending(u => u.ClickCount)
                .Take(10)
                .Select(u => new UrlShortenerStatsDto
                {
                    Id = u.Id,
                    ShortUrl = u.ShortUrl,
                    OriginalUrl = u.OriginalUrl,
                    ClickCount = u.ClickCount
                })
                .ToList();

            return new CampaignUrlStatsDto
            {
                CampaignId = campaignId,
                TotalUrls = urlList.Count,
                TotalClicks = totalClicks,
                UniqueClicks = uniqueClicks,
                TopUrls = topUrls
            };
        }

        public async Task<bool> DeleteShortenedUrlAsync(string userId, int id)
        {
            var urlShortener = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                u.Id == id && !u.IsDeleted);

            if (urlShortener == null)
                return false;

            // Load campaign to verify ownership
            var campaign = await _campaignRepository.GetByIdAsync(urlShortener.CampaignId);
            if (campaign == null || campaign.UserId != userId)
                return false;

            urlShortener.IsDeleted = true;
            urlShortener.UpdatedAt = DateTime.UtcNow;

            _urlShortenerRepository.Update(urlShortener);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Deleted shortened URL {id}");

            return true;
        }

        // Private helper methods
        private async Task<string> GenerateUniqueShortCodeAsync()
        {
            for (int i = 0; i < MaxGenerationAttempts; i++)
            {
                var shortCode = GenerateShortCode(ShortCodeLength);
                var existing = await _urlShortenerRepository.FirstOrDefaultAsync(u =>
                    u.ShortCode == shortCode && !u.IsDeleted);

                if (existing == null)
                    return shortCode;
            }

            throw new InvalidOperationException("Failed to generate unique short code");
        }

        private string GenerateShortCode(int length)
        {
            // Use cryptographically secure random number generator
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
            }

            return new string(result);
        }

        private bool IsValidShortCode(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return false;

            if (shortCode.Length < ShortCodeMinLength || shortCode.Length > ShortCodeMaxLength)
                return false;

            return shortCode.All(c => char.IsLetterOrDigit(c));
        }
    }
}
