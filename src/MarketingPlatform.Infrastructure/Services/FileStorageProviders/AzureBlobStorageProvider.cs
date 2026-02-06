using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Infrastructure.Services.FileStorageProviders
{
    public class AzureBlobStorageProvider : IFileStorageProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobStorageProvider> _logger;
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly string? _containerName;

        public string ProviderName => "Azure";

        public AzureBlobStorageProvider(IConfiguration configuration, ILogger<AzureBlobStorageProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var connectionString = _configuration["FileStorage:Azure:ConnectionString"];
            _containerName = _configuration["FileStorage:Azure:ContainerName"];

            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(_containerName))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _logger.LogInformation("Azure Blob Storage provider initialized");
            }
            else
            {
                _logger.LogWarning("Azure Blob Storage not configured");
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobName = string.IsNullOrEmpty(folder) 
                ? $"{Guid.NewGuid()}_{fileName}" 
                : $"{folder}/{Guid.NewGuid()}_{fileName}";

            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            _logger.LogInformation("File uploaded to Azure Blob Storage: {BlobName}", blobName);
            return blobName;
        }

        public async Task<Stream> DownloadAsync(string fileKey)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileKey);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<bool> DeleteAsync(string fileKey)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileKey);

            var result = await blobClient.DeleteIfExistsAsync();
            
            if (result.Value)
            {
                _logger.LogInformation("File deleted from Azure Blob Storage: {FileKey}", fileKey);
            }

            return result.Value;
        }

        public async Task<bool> ExistsAsync(string fileKey)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileKey);

            var response = await blobClient.ExistsAsync();
            return response.Value;
        }

        public async Task<string> GetUrlAsync(string fileKey, int expiryMinutes = 60)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileKey);

            // Generate SAS token for temporary access
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = fileKey,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }

            // Fallback to direct URL (requires public access)
            return blobClient.Uri.ToString();
        }

        public async Task<IEnumerable<string>> ListAsync(string? folder = null)
        {
            EnsureConfigured();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(_containerName);
            var prefix = string.IsNullOrEmpty(folder) ? null : folder;

            var blobNames = new List<string>();
            
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobNames.Add(blobItem.Name);
            }

            return blobNames;
        }

        private void EnsureConfigured()
        {
            if (_blobServiceClient == null || string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Azure Blob Storage is not configured. Please configure FileStorage:Azure:ConnectionString and FileStorage:Azure:ContainerName in appsettings.json");
            }
        }
    }
}
