using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Infrastructure.Services.FileStorageProviders
{
    public class LocalFileStorageProvider : IFileStorageProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageProvider> _logger;
        private readonly string _basePath;

        public string ProviderName => "Local";

        public LocalFileStorageProvider(IConfiguration configuration, ILogger<LocalFileStorageProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _basePath = _configuration["FileStorage:Local:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            // Ensure base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("Created local file storage directory: {BasePath}", _basePath);
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            var folderPath = string.IsNullOrEmpty(folder) ? _basePath : Path.Combine(_basePath, folder);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(folderPath, uniqueFileName);
            var fileKey = string.IsNullOrEmpty(folder) ? uniqueFileName : Path.Combine(folder, uniqueFileName);

            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            _logger.LogInformation("File uploaded to local storage: {FileKey}", fileKey);
            return fileKey;
        }

        public async Task<Stream> DownloadAsync(string fileKey)
        {
            var filePath = Path.Combine(_basePath, fileKey);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileKey}");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task<bool> DeleteAsync(string fileKey)
        {
            var filePath = Path.Combine(_basePath, fileKey);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted from local storage: {FileKey}", fileKey);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<bool> ExistsAsync(string fileKey)
        {
            var filePath = Path.Combine(_basePath, fileKey);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<string> GetUrlAsync(string fileKey, int expiryMinutes = 60)
        {
            // For local storage, return a relative URL that would be served by the application
            // In production, you'd configure a static file middleware to serve these files
            var url = $"/uploads/{fileKey}";
            return Task.FromResult(url);
        }

        public Task<IEnumerable<string>> ListAsync(string? folder = null)
        {
            var folderPath = string.IsNullOrEmpty(folder) ? _basePath : Path.Combine(_basePath, folder);
            
            if (!Directory.Exists(folderPath))
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(_basePath, f).Replace("\\", "/"));
            
            return Task.FromResult(files);
        }
    }
}
