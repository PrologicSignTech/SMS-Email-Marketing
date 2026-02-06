using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Infrastructure.Services.FileStorageProviders
{
    public class S3FileStorageProvider : IFileStorageProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<S3FileStorageProvider> _logger;
        private readonly IAmazonS3? _s3Client;
        private readonly string? _bucketName;

        public string ProviderName => "S3";

        public S3FileStorageProvider(IConfiguration configuration, ILogger<S3FileStorageProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var accessKey = _configuration["FileStorage:S3:AccessKey"];
            var secretKey = _configuration["FileStorage:S3:SecretKey"];
            var region = _configuration["FileStorage:S3:Region"];
            _bucketName = _configuration["FileStorage:S3:BucketName"];

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey) && 
                !string.IsNullOrEmpty(region) && !string.IsNullOrEmpty(_bucketName))
            {
                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
                };

                _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
                _logger.LogInformation("S3 File Storage provider initialized");
            }
            else
            {
                _logger.LogWarning("S3 File Storage not configured");
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            EnsureConfigured();

            var key = string.IsNullOrEmpty(folder) 
                ? $"{Guid.NewGuid()}_{fileName}" 
                : $"{folder}/{Guid.NewGuid()}_{fileName}";

            var transferUtility = new TransferUtility(_s3Client);
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                BucketName = _bucketName,
                Key = key,
                ContentType = contentType,
                CannedACL = S3CannedACL.Private
            };

            await transferUtility.UploadAsync(uploadRequest);

            _logger.LogInformation("File uploaded to S3: {Key}", key);
            return key;
        }

        public async Task<Stream> DownloadAsync(string fileKey)
        {
            EnsureConfigured();

            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            var response = await _s3Client!.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<bool> DeleteAsync(string fileKey)
        {
            EnsureConfigured();

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                await _s3Client!.DeleteObjectAsync(request);
                _logger.LogInformation("File deleted from S3: {FileKey}", fileKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from S3: {FileKey}", fileKey);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string fileKey)
        {
            EnsureConfigured();

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                await _s3Client!.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public Task<string> GetUrlAsync(string fileKey, int expiryMinutes = 60)
        {
            EnsureConfigured();

            // Generate pre-signed URL
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Protocol = Protocol.HTTPS
            };

            var url = _s3Client!.GetPreSignedURL(request);
            return Task.FromResult(url);
        }

        public async Task<IEnumerable<string>> ListAsync(string? folder = null)
        {
            EnsureConfigured();

            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = folder
            };

            var response = await _s3Client!.ListObjectsV2Async(request);
            return response.S3Objects.Select(obj => obj.Key);
        }

        private void EnsureConfigured()
        {
            if (_s3Client == null || string.IsNullOrEmpty(_bucketName))
            {
                throw new InvalidOperationException("S3 File Storage is not configured. Please configure FileStorage:S3:AccessKey, FileStorage:S3:SecretKey, FileStorage:S3:Region, and FileStorage:S3:BucketName in appsettings.json");
            }
        }
    }
}
