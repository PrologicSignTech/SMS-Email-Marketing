using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;

namespace MarketingPlatform.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IEmailProvider _emailProvider;
        private readonly IRepository<Role> _roleRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IUserRoleRepository userRoleRepository,
            IEmailProvider emailProvider,
            IRepository<Role> roleRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _userRoleRepository = userRoleRepository;
            _emailProvider = emailProvider;
            _roleRepository = roleRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }

            // Assign default Identity role
            await _userManager.AddToRoleAsync(user, "User");

            // Assign custom role with permissions so menus and features are visible
            var customUserRole = await _roleRepository.FirstOrDefaultAsync(r => r.Name == "User" && r.IsActive);
            if (customUserRole != null)
            {
                await _userRoleRepository.AssignRoleToUserAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = customUserRole.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("Custom 'User' role not found in database. New user {Email} will have no permissions.", user.Email);
            }

            // Generate and send OTP for email verification
            await GenerateAndSendOtpAsync(user);

            _logger.LogInformation("New user registered: {Email}. OTP sent for verification.", user.Email);

            var token = await _tokenService.GenerateJwtTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            var expiryMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"]);
            var permissions = await _userRoleRepository.GetUserPermissionsAsync(user.Id);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Roles = new List<string> { "User" },
                Permissions = permissions
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new Exception("Account is deactivated");
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);
            
            if (!result.Succeeded)
            {
                throw new Exception("Invalid email or password");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.GenerateJwtTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            var expiryMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"]);
            var permissions = await _userRoleRepository.GetUserPermissionsAsync(user.Id);

            _logger.LogInformation($"User logged in: {user.Email}");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Roles = roles.ToList(),
                Permissions = permissions
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Invalid token");
            }

            var isValid = await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
            if (!isValid)
            {
                throw new Exception("Invalid refresh token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new Exception("User not found or inactive");
            }

            await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

            var newToken = await _tokenService.GenerateJwtTokenAsync(user);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
            await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            var roles = await _userManager.GetRolesAsync(user);
            var expiryMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"]);
            var permissions = await _userRoleRepository.GetUserPermissionsAsync(user.Id);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = newToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Roles = roles.ToList(),
                Permissions = permissions
            };
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation($"User logged out: {userId}");
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password change failed: {errors}");
            }

            _logger.LogInformation($"Password changed for user: {user.Email}");
            return true;
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email is already verified");
            }

            if (string.IsNullOrEmpty(user.EmailOtp) || user.EmailOtpExpiresAt == null)
            {
                throw new Exception("No verification code found. Please request a new one.");
            }

            if (DateTime.UtcNow > user.EmailOtpExpiresAt)
            {
                throw new Exception("Verification code has expired. Please request a new one.");
            }

            if (user.EmailOtpAttempts >= 5)
            {
                throw new Exception("Too many failed attempts. Please request a new verification code.");
            }

            if (user.EmailOtp != request.Otp)
            {
                user.EmailOtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new Exception("Invalid verification code");
            }

            // OTP is valid — confirm email
            user.EmailConfirmed = true;
            user.EmailOtp = null;
            user.EmailOtpExpiresAt = null;
            user.EmailOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Email verified for user: {Email}", user.Email);
            return true;
        }

        public async Task<bool> ResendOtpAsync(ResendOtpRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email is already verified");
            }

            await GenerateAndSendOtpAsync(user);

            _logger.LogInformation("OTP resent for user: {Email}", user.Email);
            return true;
        }

        private async Task GenerateAndSendOtpAsync(ApplicationUser user)
        {
            // Generate a 6-digit OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.EmailOtp = otp;
            user.EmailOtpExpiresAt = DateTime.UtcNow.AddMinutes(5); // 5 minute expiry
            user.EmailOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            // Send OTP email
            var subject = "Verify your email - Marketing Platform";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                    <h2 style='color: #667eea;'>Email Verification</h2>
                    <p>Hi {user.FirstName ?? "there"},</p>
                    <p>Your verification code is:</p>
                    <div style='background: #f3f4f6; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #333;'>{otp}</span>
                    </div>
                    <p>This code will expire in <strong>5 minutes</strong>.</p>
                    <p>If you didn't create an account, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='color: #999; font-size: 12px;'>Marketing Platform</p>
                </div>";
            var textBody = $"Your verification code is: {otp}. It will expire in 5 minutes.";

            var (success, _, error, _) = await _emailProvider.SendEmailAsync(user.Email!, subject, textBody, htmlBody);

            if (!success)
            {
                _logger.LogWarning("Failed to send OTP email to {Email}: {Error}", user.Email, error);
                // Don't throw — user is already created, OTP is saved, they can resend
            }
            else
            {
                _logger.LogInformation("OTP email sent to {Email}: {Otp}", user.Email, otp);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
