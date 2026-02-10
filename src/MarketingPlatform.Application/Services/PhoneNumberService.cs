using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.PhoneNumber;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class PhoneNumberService : IPhoneNumberService
    {
        private readonly IRepository<PhoneNumber> _phoneNumberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PhoneNumberService> _logger;

        public PhoneNumberService(
            IRepository<PhoneNumber> phoneNumberRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PhoneNumberService> logger)
        {
            _phoneNumberRepository = phoneNumberRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResult<PhoneNumberDto>> GetAllAsync(PagedRequest request)
        {
            var query = (await _phoneNumberRepository.FindAsync(p => !p.IsDeleted)).AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Number.ToLower().Contains(searchLower) ||
                    (p.FriendlyName != null && p.FriendlyName.ToLower().Contains(searchLower)));
            }

            var totalCount = query.Count();
            query = query.OrderByDescending(p => p.CreatedAt);

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<PhoneNumberDto>>(items);

            return new PaginatedResult<PhoneNumberDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<PhoneNumberDto?> GetByIdAsync(int id)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            return phoneNumber == null ? null : _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<List<PhoneNumberDto>> GetByUserAsync(string userId)
        {
            var phoneNumbers = await _phoneNumberRepository.FindAsync(p =>
                p.AssignedToUserId == userId
                && (p.Status == PhoneNumberStatus.Active || p.Status == PhoneNumberStatus.Available)
                && !p.IsDeleted);
            return _mapper.Map<List<PhoneNumberDto>>(phoneNumbers.ToList());
        }

        public async Task<List<PhoneNumberDto>> GetAvailableAsync()
        {
            var phoneNumbers = await _phoneNumberRepository.FindAsync(p =>
                p.Status == PhoneNumberStatus.Available && p.AssignedToUserId == null && !p.IsDeleted);
            return _mapper.Map<List<PhoneNumberDto>>(phoneNumbers.ToList());
        }

        public async Task<PhoneNumberDto> CreateAsync(CreatePhoneNumberDto dto)
        {
            var existing = await _phoneNumberRepository.FirstOrDefaultAsync(p =>
                p.Number == dto.Number && !p.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Phone number '{dto.Number}' already exists.");

            var phoneNumber = _mapper.Map<PhoneNumber>(dto);
            phoneNumber.Status = PhoneNumberStatus.Available;

            await _phoneNumberRepository.AddAsync(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<PhoneNumberDto> UpdateAsync(int id, UpdatePhoneNumberDto dto)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (phoneNumber == null)
                throw new InvalidOperationException("Phone number not found.");

            if (dto.FriendlyName != null) phoneNumber.FriendlyName = dto.FriendlyName;
            if (dto.Capabilities.HasValue) phoneNumber.Capabilities = dto.Capabilities.Value;
            if (dto.Notes != null) phoneNumber.Notes = dto.Notes;
            phoneNumber.UpdatedAt = DateTime.UtcNow;

            _phoneNumberRepository.Update(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<PhoneNumberDto> AssignAsync(int id, string userId)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (phoneNumber == null)
                throw new InvalidOperationException("Phone number not found.");

            phoneNumber.AssignedToUserId = userId;
            phoneNumber.AssignedAt = DateTime.UtcNow;
            phoneNumber.Status = PhoneNumberStatus.Active;
            phoneNumber.UpdatedAt = DateTime.UtcNow;

            _phoneNumberRepository.Update(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<PhoneNumberDto> UnassignAsync(int id)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (phoneNumber == null)
                throw new InvalidOperationException("Phone number not found.");

            phoneNumber.AssignedToUserId = null;
            phoneNumber.AssignedAt = null;
            phoneNumber.Status = PhoneNumberStatus.Available;
            phoneNumber.UpdatedAt = DateTime.UtcNow;

            _phoneNumberRepository.Update(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<PhoneNumberDto> PurchaseAsync(string userId, PurchasePhoneNumberDto dto)
        {
            var existing = await _phoneNumberRepository.FirstOrDefaultAsync(p =>
                p.Number == dto.Number && !p.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Phone number '{dto.Number}' already exists.");

            var phoneNumber = new PhoneNumber
            {
                Number = dto.Number,
                FriendlyName = dto.FriendlyName,
                NumberType = dto.NumberType,
                Capabilities = dto.Capabilities,
                Country = dto.Country,
                Region = dto.Region,
                Status = PhoneNumberStatus.Active,
                AssignedToUserId = userId,
                PurchasedByUserId = userId,
                PurchasedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                MonthlyRate = 1.00m // Default rate
            };

            await _phoneNumberRepository.AddAsync(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PhoneNumberDto>(phoneNumber);
        }

        public async Task<bool> ReleaseAsync(int id)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (phoneNumber == null) return false;

            phoneNumber.Status = PhoneNumberStatus.Released;
            phoneNumber.AssignedToUserId = null;
            phoneNumber.AssignedAt = null;
            phoneNumber.UpdatedAt = DateTime.UtcNow;

            _phoneNumberRepository.Update(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var phoneNumber = await _phoneNumberRepository.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (phoneNumber == null) return false;

            phoneNumber.IsDeleted = true;
            phoneNumber.UpdatedAt = DateTime.UtcNow;

            _phoneNumberRepository.Update(phoneNumber);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
