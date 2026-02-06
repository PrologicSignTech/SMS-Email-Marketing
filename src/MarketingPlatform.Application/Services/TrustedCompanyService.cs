using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class TrustedCompanyService : ITrustedCompanyService
    {
        private readonly IRepository<TrustedCompany> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrustedCompanyService> _logger;

        public TrustedCompanyService(
            IRepository<TrustedCompany> repository,
            IUnitOfWork unitOfWork,
            ILogger<TrustedCompanyService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TrustedCompany>> GetAllActiveAsync()
        {
            var companies = await _repository.FindAsync(c => c.IsActive && !c.IsDeleted);
            return companies.OrderBy(c => c.DisplayOrder);
        }

        public async Task<TrustedCompany?> GetByIdAsync(int id)
        {
            var company = await _repository.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            return company;
        }

        public async Task<TrustedCompany> CreateAsync(TrustedCompany company)
        {
            company.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created trusted company: {CompanyName}", company.CompanyName);

            return company;
        }

        public async Task<TrustedCompany> UpdateAsync(int id, TrustedCompany company)
        {
            var existing = await _repository.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Trusted company not found");

            existing.CompanyName = company.CompanyName;
            existing.LogoUrl = company.LogoUrl;
            existing.WebsiteUrl = company.WebsiteUrl;
            existing.DisplayOrder = company.DisplayOrder;
            existing.IsActive = company.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated trusted company: {CompanyName}", company.CompanyName);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var company = await _repository.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (company == null)
                throw new InvalidOperationException("Trusted company not found");

            company.IsDeleted = true;
            company.UpdatedAt = DateTime.UtcNow;

            _repository.Update(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted trusted company: {CompanyName}", company.CompanyName);

            return true;
        }
    }
}
