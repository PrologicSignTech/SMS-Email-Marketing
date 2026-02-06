using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class PlatformSettingService : IPlatformSettingService
    {
        private readonly IRepository<PlatformSetting> _settingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformSettingService> _logger;

        public PlatformSettingService(
            IRepository<PlatformSetting> settingRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PlatformSettingService> logger)
        {
            _settingRepository = settingRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlatformSettingDto?> GetSettingByIdAsync(int id)
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            return setting != null ? _mapper.Map<PlatformSettingDto>(setting) : null;
        }

        public async Task<PlatformSettingDto?> GetSettingByKeyAsync(string key)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);
            return setting != null ? _mapper.Map<PlatformSettingDto>(setting) : null;
        }

        public async Task<PaginatedResult<PlatformSettingDto>> GetSettingsAsync(PagedRequest request)
        {
            var query = _settingRepository.GetQueryable().Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(s =>
                    s.Key.Contains(request.SearchTerm) ||
                    (s.Description != null && s.Description.Contains(request.SearchTerm)) ||
                    (s.Category != null && s.Category.Contains(request.SearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var settings = await query
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var settingDtos = _mapper.Map<List<PlatformSettingDto>>(settings);

            return new PaginatedResult<PlatformSettingDto>
            {
                Items = settingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<PlatformSettingDto>> GetSettingsByCategoryAsync(string category)
        {
            var settings = await _settingRepository.FindAsync(s => 
                s.Category == category && !s.IsDeleted);
            return _mapper.Map<List<PlatformSettingDto>>(settings);
        }

        public async Task<PlatformSettingDto> CreateSettingAsync(CreatePlatformSettingDto dto, string userId)
        {
            // Check if key already exists
            var existingSetting = await _settingRepository.FirstOrDefaultAsync(s => s.Key == dto.Key);
            if (existingSetting != null)
            {
                throw new InvalidOperationException($"Setting with key '{dto.Key}' already exists.");
            }

            var setting = _mapper.Map<PlatformSetting>(dto);
            setting.ModifiedBy = userId;
            setting.CreatedAt = DateTime.UtcNow;

            await _settingRepository.AddAsync(setting);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Platform setting created: {Key} by user {UserId}", setting.Key, userId);

            return _mapper.Map<PlatformSettingDto>(setting);
        }

        public async Task<bool> UpdateSettingAsync(int id, UpdatePlatformSettingDto dto, string userId)
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null || setting.IsDeleted)
            {
                return false;
            }

            if (setting.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot update read-only setting.");
            }

            setting.Value = dto.Value;
            setting.Description = dto.Description ?? setting.Description;
            setting.Category = dto.Category ?? setting.Category;
            setting.ModifiedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;

            _settingRepository.Update(setting);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Platform setting updated: {Key} by user {UserId}", setting.Key, userId);

            return true;
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null || setting.IsDeleted)
            {
                return false;
            }

            if (setting.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot delete read-only setting.");
            }

            setting.IsDeleted = true;
            setting.UpdatedAt = DateTime.UtcNow;

            _settingRepository.Update(setting);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Platform setting deleted: {Key}", setting.Key);

            return true;
        }

        public async Task<T?> GetSettingValueAsync<T>(string key, T? defaultValue = default)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);
            if (setting == null)
            {
                return defaultValue;
            }

            try
            {
                return setting.DataType switch
                {
                    SettingDataType.String => (T)(object)setting.Value,
                    SettingDataType.Integer => int.TryParse(setting.Value, out var intValue) ? (T)(object)intValue : defaultValue,
                    SettingDataType.Boolean => bool.TryParse(setting.Value, out var boolValue) ? (T)(object)boolValue : defaultValue,
                    SettingDataType.Decimal => decimal.TryParse(setting.Value, out var decimalValue) ? (T)(object)decimalValue : defaultValue,
                    SettingDataType.Json => JsonConvert.DeserializeObject<T>(setting.Value) ?? defaultValue,
                    _ => defaultValue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting setting value for key {Key}", key);
                return defaultValue;
            }
        }

        public async Task<bool> SetSettingValueAsync(string key, object value, string userId)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);
            if (setting == null)
            {
                return false;
            }

            if (setting.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot update read-only setting.");
            }

            setting.Value = setting.DataType switch
            {
                SettingDataType.Json => JsonConvert.SerializeObject(value),
                _ => value.ToString() ?? string.Empty
            };

            setting.ModifiedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;

            _settingRepository.Update(setting);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
