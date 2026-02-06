using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;

namespace MarketingPlatform.Infrastructure.Services
{
    public class OAuth2Service : IOAuth2Service
    {
        private readonly ApplicationDbContext _context;
        private readonly IExternalAuthProviderRepository _providerRepository;
        private readonly IUserExternalLoginRepository _externalLoginRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<OAuth2Service> _logger;
        private readonly Dictionary<string, IOAuth2Provider> _providers;

        public OAuth2Service(
            ApplicationDbContext context,
            IExternalAuthProviderRepository providerRepository,
            IUserExternalLoginRepository externalLoginRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ILogger<OAuth2Service> logger,
            IEnumerable<IOAuth2Provider> providers)
        {
            _context = context;
            _providerRepository = providerRepository;
            _externalLoginRepository = externalLoginRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _providers = providers.ToDictionary(p => p.ProviderName, p => p);
        }

        public async Task<string> GetAuthorizationUrlAsync(string providerName, string? redirectUri = null, string? state = null)
        {
            var provider = await GetProviderConfigAsync(providerName);
            
            if (!_providers.TryGetValue(providerName, out var oauthProvider))
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is not supported");
            }

            redirectUri ??= provider.CallbackPath;
            state ??= Guid.NewGuid().ToString();

            return oauthProvider.BuildAuthorizationUrl(provider, redirectUri, state);
        }

        public async Task<AuthResponseDto> HandleCallbackAsync(string providerName, OAuth2CallbackDto callback)
        {
            if (!string.IsNullOrEmpty(callback.Error))
            {
                _logger.LogError("OAuth2 error from {Provider}: {Error} - {ErrorDescription}", 
                    providerName, callback.Error, callback.ErrorDescription);
                throw new InvalidOperationException($"OAuth2 error: {callback.Error}");
            }

            var provider = await GetProviderConfigAsync(providerName);
            
            if (!_providers.TryGetValue(providerName, out var oauthProvider))
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is not supported");
            }

            // Exchange code for tokens
            var tokens = await oauthProvider.ExchangeCodeForTokenAsync(provider, callback.Code, provider.CallbackPath);

            // Get user info from provider
            var externalUserInfo = await oauthProvider.GetUserInfoAsync(provider, tokens.AccessToken);

            // Find or create user
            var existingLogin = await _externalLoginRepository.GetByProviderAsync(providerName, externalUserInfo.ProviderUserId);
            
            ApplicationUser user;
            
            if (existingLogin != null)
            {
                // User already linked this external account
                user = existingLogin.User;
                
                // Update tokens
                existingLogin.AccessToken = tokens.AccessToken;
                existingLogin.RefreshToken = tokens.RefreshToken;
                existingLogin.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
                existingLogin.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Check if user exists by email
                user = await _userManager.FindByEmailAsync(externalUserInfo.Email);
                
                if (user == null)
                {
                    // Create new user
                    user = new ApplicationUser
                    {
                        UserName = externalUserInfo.Email,
                        Email = externalUserInfo.Email,
                        FirstName = externalUserInfo.FirstName ?? string.Empty,
                        LastName = externalUserInfo.LastName ?? string.Empty,
                        EmailConfirmed = true, // Trust external provider email verification
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create user: {errors}");
                    }

                    // Assign default role
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Link external account
                var externalLogin = new UserExternalLogin
                {
                    UserId = user.Id,
                    ProviderId = provider.Id,
                    ProviderUserId = externalUserInfo.ProviderUserId,
                    ProviderUserName = externalUserInfo.DisplayName ?? externalUserInfo.Email,
                    ProviderEmail = externalUserInfo.Email,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn),
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                await _context.UserExternalLogins.AddAsync(externalLogin);
                await _context.SaveChangesAsync();
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate JWT tokens for our application
            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = await _tokenService.GenerateJwtTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            _logger.LogInformation("User {Email} logged in via {Provider}", user.Email, providerName);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Token = jwtToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                Roles = roles.ToList()
            };
        }

        public async Task<ExternalUserInfoDto> GetUserInfoAsync(string providerName, string accessToken)
        {
            var provider = await GetProviderConfigAsync(providerName);
            
            if (!_providers.TryGetValue(providerName, out var oauthProvider))
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is not supported");
            }

            return await oauthProvider.GetUserInfoAsync(provider, accessToken);
        }

        public async Task<bool> LinkExternalAccountAsync(string userId, string providerName, string providerUserId, OAuth2TokenResponseDto tokens)
        {
            var provider = await GetProviderConfigAsync(providerName);
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Check if already linked
            var existing = await _externalLoginRepository.GetByProviderAsync(providerName, providerUserId);
            if (existing != null)
            {
                throw new InvalidOperationException("This external account is already linked to another user");
            }

            var externalLogin = new UserExternalLogin
            {
                UserId = userId,
                ProviderId = provider.Id,
                ProviderUserId = providerUserId,
                ProviderUserName = user.Email!,
                ProviderEmail = user.Email,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _context.UserExternalLogins.AddAsync(externalLogin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("External account {Provider} linked to user {UserId}", providerName, userId);
            return true;
        }

        public async Task<bool> UnlinkExternalAccountAsync(string userId, string providerName)
        {
            return await _externalLoginRepository.RemoveByProviderAsync(userId, providerName);
        }

        public async Task<OAuth2TokenResponseDto> RefreshExternalTokenAsync(string userId, string providerName)
        {
            var externalLogins = await _externalLoginRepository.GetByUserIdAsync(userId);
            var externalLogin = externalLogins.FirstOrDefault(el => el.Provider.Name == providerName);

            if (externalLogin == null || string.IsNullOrEmpty(externalLogin.RefreshToken))
            {
                throw new InvalidOperationException("External login not found or refresh token not available");
            }

            if (!_providers.TryGetValue(providerName, out var oauthProvider))
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is not supported");
            }

            var tokens = await oauthProvider.RefreshTokenAsync(externalLogin.Provider, externalLogin.RefreshToken);

            // Update stored tokens
            externalLogin.AccessToken = tokens.AccessToken;
            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                externalLogin.RefreshToken = tokens.RefreshToken;
            }
            externalLogin.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
            await _context.SaveChangesAsync();

            return tokens;
        }

        public async Task<IEnumerable<ExternalAuthProviderDto>> GetEnabledProvidersAsync()
        {
            var providers = await _providerRepository.GetEnabledProvidersAsync();
            
            return providers.Select(p => new ExternalAuthProviderDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                ProviderType = p.ProviderType,
                IsEnabled = p.IsEnabled,
                IsDefault = p.IsDefault
            });
        }

        // Admin Provider Management Methods
        public async Task<IEnumerable<ExternalAuthProvider>> GetAllProvidersAsync()
        {
            return await _providerRepository.GetAllAsync();
        }

        public async Task<ExternalAuthProvider?> GetProviderByIdAsync(int id)
        {
            return await _providerRepository.GetByIdAsync(id);
        }

        public async Task<ExternalAuthProvider> CreateProviderAsync(ExternalAuthProvider provider)
        {
            provider.CreatedAt = DateTime.UtcNow;
            await _providerRepository.AddAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Created new OAuth2 provider: {ProviderName}", provider.Name);
            return provider;
        }

        public async Task<ExternalAuthProvider?> UpdateProviderAsync(int id, ExternalAuthProvider updatedProvider)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            
            if (provider == null)
                return null;

            // Update properties
            provider.DisplayName = updatedProvider.DisplayName;
            provider.ClientId = updatedProvider.ClientId;
            provider.ClientSecret = updatedProvider.ClientSecret;
            provider.Authority = updatedProvider.Authority;
            provider.TenantId = updatedProvider.TenantId;
            provider.Domain = updatedProvider.Domain;
            provider.Region = updatedProvider.Region;
            provider.UserPoolId = updatedProvider.UserPoolId;
            provider.CallbackPath = updatedProvider.CallbackPath;
            provider.Scopes = updatedProvider.Scopes;
            provider.IsEnabled = updatedProvider.IsEnabled;
            provider.IsDefault = updatedProvider.IsDefault;
            provider.ConfigurationJson = updatedProvider.ConfigurationJson;
            provider.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Updated OAuth2 provider: {ProviderName} (ID: {ProviderId})", provider.Name, id);
            return provider;
        }

        public async Task<bool> DeleteProviderAsync(int id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            
            if (provider == null)
                return false;

            // Check if provider has any linked accounts
            var hasLinkedAccounts = await _context.UserExternalLogins
                .AnyAsync(uel => uel.ProviderId == id);

            if (hasLinkedAccounts)
            {
                throw new InvalidOperationException(
                    "Cannot delete provider with linked user accounts. Disable it instead or unlink all accounts first.");
            }

            _providerRepository.Remove(provider);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Deleted OAuth2 provider: {ProviderName} (ID: {ProviderId})", provider.Name, id);
            return true;
        }

        public async Task<bool> ToggleProviderStatusAsync(int id, bool isEnabled)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            
            if (provider == null)
                return false;

            provider.IsEnabled = isEnabled;
            provider.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Toggled OAuth2 provider {ProviderName} status to {Status}", 
                provider.Name, isEnabled ? "enabled" : "disabled");
            return true;
        }

        private async Task<ExternalAuthProvider> GetProviderConfigAsync(string providerName)
        {
            var provider = await _providerRepository.GetByNameAsync(providerName);
            
            if (provider == null)
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is not configured");
            }

            if (!provider.IsEnabled)
            {
                throw new InvalidOperationException($"OAuth2 provider '{providerName}' is disabled");
            }

            return provider;
        }
    }
}
