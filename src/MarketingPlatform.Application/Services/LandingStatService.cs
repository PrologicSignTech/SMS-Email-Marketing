using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class LandingStatService : ILandingStatService
    {
        private readonly IRepository<LandingStat> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LandingStatService> _logger;

        public LandingStatService(
            IRepository<LandingStat> repository,
            IUnitOfWork unitOfWork,
            ILogger<LandingStatService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<LandingStat>> GetAllActiveAsync()
        {
            var stats = await _repository.FindAsync(s =>
                s.IsActive && s.ShowOnLanding && !s.IsDeleted);
            return stats.OrderBy(s => s.DisplayOrder);
        }

        public async Task<LandingStat?> GetByIdAsync(int id)
        {
            var stat = await _repository.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return stat;
        }

        public async Task<LandingStat> CreateAsync(LandingStat stat)
        {
            stat.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(stat);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created landing stat: {Label}", stat.Label);

            return stat;
        }

        public async Task<LandingStat> UpdateAsync(int id, LandingStat stat)
        {
            var existing = await _repository.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Stat not found");

            existing.Value = stat.Value;
            existing.Label = stat.Label;
            existing.Description = stat.Description;
            existing.IconClass = stat.IconClass;
            existing.ColorClass = stat.ColorClass;
            existing.CounterTarget = stat.CounterTarget;
            existing.CounterSuffix = stat.CounterSuffix;
            existing.CounterPrefix = stat.CounterPrefix;
            existing.DisplayOrder = stat.DisplayOrder;
            existing.IsActive = stat.IsActive;
            existing.ShowOnLanding = stat.ShowOnLanding;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated landing stat: {Label}", stat.Label);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var stat = await _repository.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (stat == null)
                throw new InvalidOperationException("Stat not found");

            stat.IsDeleted = true;
            stat.UpdatedAt = DateTime.UtcNow;

            _repository.Update(stat);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted landing stat: {Label}", stat.Label);

            return true;
        }
    }
}
