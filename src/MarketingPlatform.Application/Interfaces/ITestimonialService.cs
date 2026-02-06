using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Application.Interfaces
{
    public interface ITestimonialService
    {
        Task<IEnumerable<Testimonial>> GetAllActiveAsync();
        Task<Testimonial?> GetByIdAsync(int id);
        Task<Testimonial> CreateAsync(Testimonial testimonial);
        Task<Testimonial> UpdateAsync(int id, Testimonial testimonial);
        Task<bool> DeleteAsync(int id);
    }
}
