using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class UseCaseService : IUseCaseService
    {
        private readonly IRepository<UseCase> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UseCaseService> _logger;

        public UseCaseService(
            IRepository<UseCase> repository,
            IUnitOfWork unitOfWork,
            ILogger<UseCaseService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<UseCase>> GetAllActiveAsync()
        {
            var useCases = await _repository.FindAsync(u => u.IsActive && !u.IsDeleted);
            return useCases.OrderBy(u => u.DisplayOrder);
        }

        public async Task<UseCase?> GetByIdAsync(int id)
        {
            var useCase = await _repository.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            return useCase;
        }

        public async Task<IEnumerable<UseCase>> GetByIndustryAsync(string industry)
        {
            var useCases = await _repository.FindAsync(u =>
                u.IsActive && !u.IsDeleted && u.Industry == industry);
            return useCases.OrderBy(u => u.DisplayOrder);
        }

        public async Task<UseCase> CreateAsync(UseCase useCase)
        {
            useCase.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(useCase);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created use case: {Title}", useCase.Title);

            return useCase;
        }

        public async Task<UseCase> UpdateAsync(int id, UseCase useCase)
        {
            var existing = await _repository.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Use case not found");

            existing.Title = useCase.Title;
            existing.Description = useCase.Description;
            existing.IconClass = useCase.IconClass;
            existing.ColorClass = useCase.ColorClass;
            existing.Industry = useCase.Industry;
            existing.ImageUrl = useCase.ImageUrl;
            existing.ResultsText = useCase.ResultsText;
            existing.DisplayOrder = useCase.DisplayOrder;
            existing.IsActive = useCase.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated use case: {Title}", useCase.Title);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var useCase = await _repository.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (useCase == null)
                throw new InvalidOperationException("Use case not found");

            useCase.IsDeleted = true;
            useCase.UpdatedAt = DateTime.UtcNow;

            _repository.Update(useCase);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted use case: {Title}", useCase.Title);

            return true;
        }
    }
}
