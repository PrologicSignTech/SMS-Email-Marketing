using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class TestimonialService : ITestimonialService
    {
        private readonly IRepository<Testimonial> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TestimonialService> _logger;

        public TestimonialService(
            IRepository<Testimonial> repository,
            IUnitOfWork unitOfWork,
            ILogger<TestimonialService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Testimonial>> GetAllActiveAsync()
        {
            var testimonials = await _repository.FindAsync(t => t.IsActive && !t.IsDeleted);
            return testimonials.OrderBy(t => t.DisplayOrder);
        }

        public async Task<Testimonial?> GetByIdAsync(int id)
        {
            var testimonial = await _repository.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            return testimonial;
        }

        public async Task<Testimonial> CreateAsync(Testimonial testimonial)
        {
            testimonial.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(testimonial);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created testimonial from: {CustomerName}", testimonial.CustomerName);

            return testimonial;
        }

        public async Task<Testimonial> UpdateAsync(int id, Testimonial testimonial)
        {
            var existing = await _repository.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Testimonial not found");

            existing.CustomerName = testimonial.CustomerName;
            existing.CustomerTitle = testimonial.CustomerTitle;
            existing.CompanyName = testimonial.CompanyName;
            existing.CompanyLogo = testimonial.CompanyLogo;
            existing.AvatarUrl = testimonial.AvatarUrl;
            existing.TestimonialText = testimonial.TestimonialText;
            existing.Rating = testimonial.Rating;
            existing.DisplayOrder = testimonial.DisplayOrder;
            existing.IsActive = testimonial.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated testimonial from: {CustomerName}", testimonial.CustomerName);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var testimonial = await _repository.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (testimonial == null)
                throw new InvalidOperationException("Testimonial not found");

            testimonial.IsDeleted = true;
            testimonial.UpdatedAt = DateTime.UtcNow;

            _repository.Update(testimonial);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted testimonial from: {CustomerName}", testimonial.CustomerName);

            return true;
        }
    }
}
