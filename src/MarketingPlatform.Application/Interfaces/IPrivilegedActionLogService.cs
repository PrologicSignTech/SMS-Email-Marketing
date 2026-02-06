using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IPrivilegedActionLogService
    {
        /// <summary>
        /// Get paginated logs with filtering
        /// </summary>
        Task<(IEnumerable<PrivilegedActionLogDto> Logs, int TotalCount)> GetPagedLogsAsync(PrivilegedActionFilterDto filter);

        /// <summary>
        /// Get logs for a specific entity
        /// </summary>
        Task<IEnumerable<PrivilegedActionLogDto>> GetLogsByEntityAsync(string entityType, string entityId);

        /// <summary>
        /// Get logs for a specific user
        /// </summary>
        Task<IEnumerable<PrivilegedActionLogDto>> GetLogsByUserAsync(string userId, int limit = 100);

        /// <summary>
        /// Get critical actions within a time range
        /// </summary>
        Task<IEnumerable<PrivilegedActionLogDto>> GetCriticalActionsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Log a privileged action
        /// </summary>
        Task LogActionAsync(string performedBy, CreatePrivilegedActionLogDto request, string? ipAddress = null, string? userAgent = null, string? requestPath = null);
    }
}
