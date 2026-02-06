using Microsoft.EntityFrameworkCore;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class UserExternalLoginRepository : Repository<UserExternalLogin>, IUserExternalLoginRepository
    {
        public UserExternalLoginRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserExternalLogin?> GetByProviderAsync(string providerName, string providerUserId)
        {
            return await _context.UserExternalLogins
                .Include(uel => uel.Provider)
                .Include(uel => uel.User)
                .FirstOrDefaultAsync(uel => 
                    uel.Provider.Name == providerName && 
                    uel.ProviderUserId == providerUserId);
        }

        public async Task<IEnumerable<UserExternalLogin>> GetByUserIdAsync(string userId)
        {
            return await _context.UserExternalLogins
                .Include(uel => uel.Provider)
                .Where(uel => uel.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> RemoveByProviderAsync(string userId, string providerName)
        {
            var login = await _context.UserExternalLogins
                .Include(uel => uel.Provider)
                .FirstOrDefaultAsync(uel => 
                    uel.UserId == userId && 
                    uel.Provider.Name == providerName);
            
            if (login != null)
            {
                _context.UserExternalLogins.Remove(login);
                await _context.SaveChangesAsync();
                return true;
            }
            
            return false;
        }
    }
}
