using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.SuppressionList;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class SuppressionListService : ISuppressionListService
    {
        private readonly IRepository<SuppressionList> _suppressionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SuppressionListService> _logger;

        public SuppressionListService(
            IRepository<SuppressionList> suppressionRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SuppressionListService> logger)
        {
            _suppressionRepository = suppressionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SuppressionListDto?> GetByIdAsync(string userId, int id)
        {
            var suppression = await _suppressionRepository.FirstOrDefaultAsync(s =>
                s.Id == id && s.UserId == userId && !s.IsDeleted);

            if (suppression == null)
                return null;

            return _mapper.Map<SuppressionListDto>(suppression);
        }

        public async Task<PaginatedResult<SuppressionListDto>> GetAllAsync(string userId, PagedRequest request)
        {
            var query = (await _suppressionRepository.FindAsync(s =>
                s.UserId == userId && !s.IsDeleted)).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(s => s.PhoneOrEmail.ToLower().Contains(searchLower));
            }

            var totalCount = query.Count();

            // Apply sorting
            query = query.OrderByDescending(s => s.CreatedAt);

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<SuppressionListDto>>(items);

            return new PaginatedResult<SuppressionListDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<SuppressionListDto> CreateAsync(string userId, CreateSuppressionListDto dto)
        {
            // Check for duplicates
            var existing = await _suppressionRepository.FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.PhoneOrEmail.ToLower() == dto.PhoneOrEmail.ToLower() &&
                !s.IsDeleted);

            if (existing != null)
            {
                throw new InvalidOperationException($"'{dto.PhoneOrEmail}' is already in the suppression list.");
            }

            var suppression = _mapper.Map<SuppressionList>(dto);
            suppression.UserId = userId;

            await _suppressionRepository.AddAsync(suppression);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SuppressionListDto>(suppression);
        }

        public async Task<bool> DeleteAsync(string userId, int id)
        {
            var suppression = await _suppressionRepository.FirstOrDefaultAsync(s =>
                s.Id == id && s.UserId == userId && !s.IsDeleted);

            if (suppression == null)
                return false;

            suppression.IsDeleted = true;
            suppression.UpdatedAt = DateTime.UtcNow;

            _suppressionRepository.Update(suppression);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> BulkCreateAsync(string userId, BulkSuppressionDto dto)
        {
            int addedCount = 0;

            // Get existing suppressions to avoid duplicates
            var existingSuppressions = await _suppressionRepository.FindAsync(s =>
                s.UserId == userId && !s.IsDeleted);
            var existingSet = existingSuppressions
                .Select(s => s.PhoneOrEmail.ToLower())
                .ToHashSet();

            var suppressionsToAdd = new List<SuppressionList>();

            foreach (var phoneOrEmail in dto.PhoneOrEmails)
            {
                if (string.IsNullOrWhiteSpace(phoneOrEmail))
                    continue;

                var normalized = phoneOrEmail.ToLower();
                if (existingSet.Contains(normalized))
                    continue;

                var suppression = new SuppressionList
                {
                    UserId = userId,
                    PhoneOrEmail = phoneOrEmail,
                    Type = dto.Type,
                    Reason = dto.Reason
                };

                suppressionsToAdd.Add(suppression);
                existingSet.Add(normalized);
                addedCount++;
            }

            // Add all suppressions in a single batch
            if (suppressionsToAdd.Any())
            {
                foreach (var suppression in suppressionsToAdd)
                {
                    await _suppressionRepository.AddAsync(suppression);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return addedCount;
        }

        public async Task<bool> IsSuppressedAsync(string userId, string phoneOrEmail)
        {
            var suppression = await _suppressionRepository.FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.PhoneOrEmail.ToLower() == phoneOrEmail.ToLower() &&
                !s.IsDeleted);

            return suppression != null;
        }
    }
}
