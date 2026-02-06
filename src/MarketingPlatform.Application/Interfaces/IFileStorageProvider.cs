namespace MarketingPlatform.Application.Interfaces
{
    /// <summary>
    /// Base interface for file storage provider implementations
    /// </summary>
    public interface IFileStorageProvider
    {
        string ProviderName { get; }
        
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
        Task<Stream> DownloadAsync(string fileKey);
        Task<bool> DeleteAsync(string fileKey);
        Task<bool> ExistsAsync(string fileKey);
        Task<string> GetUrlAsync(string fileKey, int expiryMinutes = 60);
        Task<IEnumerable<string>> ListAsync(string? folder = null);
    }
}
