using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.User;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<PaginatedResult<UserDto>> GetUsersAsync(PagedRequest request);
        Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        Task<UserStatsDto> GetUserStatsAsync(string userId);
    }
}
