using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Core.Interfaces.Repositories
{
    public interface IPrivilegedActionLogRepository : IRepository<PrivilegedActionLog>
    {
        /// <summary>
        /// Get paginated logs with filtering
        /// </summary>
        Task<(IEnumerable<PrivilegedActionLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            PrivilegedActionType? actionType = null,
            string? userId = null,
            ActionSeverity? severity = null,
            string? entityType = null);

        /// <summary>
        /// Get logs for a specific entity
        /// </summary>
        Task<IEnumerable<PrivilegedActionLog>> GetByEntityAsync(string entityType, string entityId);

        /// <summary>
        /// Get logs for a specific user
        /// </summary>
        Task<IEnumerable<PrivilegedActionLog>> GetByUserAsync(string userId, int limit = 100);

        /// <summary>
        /// Get critical actions within a time range
        /// </summary>
        Task<IEnumerable<PrivilegedActionLog>> GetCriticalActionsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Log a privileged action
        /// </summary>
        Task LogActionAsync(PrivilegedActionLog log);
    }
}
