namespace MarketingPlatform.Application.Interfaces
{
    /// <summary>
    /// Interface for file storage operations with support for multiple providers
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Upload a file to storage
        /// </summary>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
        
        /// <summary>
        /// Download a file from storage
        /// </summary>
        Task<Stream> DownloadFileAsync(string fileKey);
        
        /// <summary>
        /// Delete a file from storage
        /// </summary>
        Task<bool> DeleteFileAsync(string fileKey);
        
        /// <summary>
        /// Check if a file exists
        /// </summary>
        Task<bool> FileExistsAsync(string fileKey);
        
        /// <summary>
        /// Get file URL (public or signed)
        /// </summary>
        Task<string> GetFileUrlAsync(string fileKey, int expiryMinutes = 60);
        
        /// <summary>
        /// List files in a folder
        /// </summary>
        Task<IEnumerable<string>> ListFilesAsync(string? folder = null);
    }
}
