using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class LandingFaqService : ILandingFaqService
    {
        private readonly IRepository<LandingFaq> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LandingFaqService> _logger;

        public LandingFaqService(
            IRepository<LandingFaq> repository,
            IUnitOfWork unitOfWork,
            ILogger<LandingFaqService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<LandingFaq>> GetAllActiveAsync()
        {
            var faqs = await _repository.FindAsync(f => 
                f.IsActive && f.ShowOnLanding && !f.IsDeleted);
            return faqs.OrderBy(f => f.DisplayOrder);
        }

        public async Task<LandingFaq?> GetByIdAsync(int id)
        {
            var faq = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            return faq;
        }

        public async Task<LandingFaq> CreateAsync(LandingFaq faq)
        {
            faq.CreatedAt = DateTime.UtcNow;
            
            await _repository.AddAsync(faq);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created landing FAQ: {Question}", faq.Question);

            return faq;
        }

        public async Task<LandingFaq> UpdateAsync(int id, LandingFaq faq)
        {
            var existing = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            
            if (existing == null)
                throw new InvalidOperationException("FAQ not found");

            existing.Question = faq.Question;
            existing.Answer = faq.Answer;
            existing.IconClass = faq.IconClass;
            existing.IconColor = faq.IconColor;
            existing.DisplayOrder = faq.DisplayOrder;
            existing.IsActive = faq.IsActive;
            existing.ShowOnLanding = faq.ShowOnLanding;
            existing.Category = faq.Category;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated landing FAQ: {Question}", faq.Question);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var faq = await _repository.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            
            if (faq == null)
                throw new InvalidOperationException("FAQ not found");

            faq.IsDeleted = true;
            faq.UpdatedAt = DateTime.UtcNow;

            _repository.Update(faq);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted landing FAQ: {Question}", faq.Question);

            return true;
        }
    }
}
