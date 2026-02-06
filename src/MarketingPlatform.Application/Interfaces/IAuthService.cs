using MarketingPlatform.Application.DTOs.Auth;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto request);
    }
}
