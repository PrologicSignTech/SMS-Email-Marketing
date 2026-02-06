using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly Dictionary<string, IFileStorageProvider> _providers;
        private readonly IFileStorageProvider _activeProvider;

        public FileStorageService(
            IConfiguration configuration,
            ILogger<FileStorageService> logger,
            IEnumerable<IFileStorageProvider> providers)
        {
            _configuration = configuration;
            _logger = logger;
            _providers = providers.ToDictionary(p => p.ProviderName, p => p);

            // Get active provider from configuration (default to Local)
            var activeProviderName = _configuration["FileStorage:Provider"] ?? "Local";
            
            if (!_providers.TryGetValue(activeProviderName, out var provider))
            {
                _logger.LogWarning("Configured provider '{Provider}' not found, falling back to Local", activeProviderName);
                provider = _providers["Local"];
            }

            _activeProvider = provider;
            _logger.LogInformation("File storage initialized with provider: {Provider}", _activeProvider.ProviderName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            return await _activeProvider.UploadAsync(fileStream, fileName, contentType, folder);
        }

        public async Task<Stream> DownloadFileAsync(string fileKey)
        {
            return await _activeProvider.DownloadAsync(fileKey);
        }

        public async Task<bool> DeleteFileAsync(string fileKey)
        {
            return await _activeProvider.DeleteAsync(fileKey);
        }

        public async Task<bool> FileExistsAsync(string fileKey)
        {
            return await _activeProvider.ExistsAsync(fileKey);
        }

        public async Task<string> GetFileUrlAsync(string fileKey, int expiryMinutes = 60)
        {
            return await _activeProvider.GetUrlAsync(fileKey, expiryMinutes);
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string? folder = null)
        {
            return await _activeProvider.ListAsync(folder);
        }
    }
}
