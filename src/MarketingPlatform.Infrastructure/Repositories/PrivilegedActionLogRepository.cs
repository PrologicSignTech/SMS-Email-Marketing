using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Repositories
{
    public class PrivilegedActionLogRepository : Repository<PrivilegedActionLog>, IPrivilegedActionLogRepository
    {
        public PrivilegedActionLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<PrivilegedActionLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            PrivilegedActionType? actionType = null,
            string? userId = null,
            ActionSeverity? severity = null,
            string? entityType = null)
        {
            var query = _dbSet.Include(p => p.PerformedByUser).AsQueryable();

            // Apply filters
            if (startDate.HasValue)
                query = query.Where(p => p.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Timestamp <= endDate.Value);

            if (actionType.HasValue)
                query = query.Where(p => p.ActionType == actionType.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(p => p.PerformedBy == userId);

            if (severity.HasValue)
                query = query.Where(p => p.Severity == severity.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(p => p.EntityType == entityType);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(p => p.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<IEnumerable<PrivilegedActionLog>> GetByEntityAsync(string entityType, string entityId)
        {
            return await _dbSet
                .Include(p => p.PerformedByUser)
                .Where(p => p.EntityType == entityType && p.EntityId == entityId)
                .OrderByDescending(p => p.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrivilegedActionLog>> GetByUserAsync(string userId, int limit = 100)
        {
            return await _dbSet
                .Include(p => p.PerformedByUser)
                .Where(p => p.PerformedBy == userId)
                .OrderByDescending(p => p.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrivilegedActionLog>> GetCriticalActionsAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(p => p.PerformedByUser)
                .Where(p => p.Severity == ActionSeverity.Critical 
                    && p.Timestamp >= startDate 
                    && p.Timestamp <= endDate)
                .OrderByDescending(p => p.Timestamp)
                .ToListAsync();
        }

        public async Task LogActionAsync(PrivilegedActionLog log)
        {
            await AddAsync(log);
        }
    }
}
