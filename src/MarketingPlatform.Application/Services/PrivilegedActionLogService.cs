using AutoMapper;
using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class PrivilegedActionLogService : IPrivilegedActionLogService
    {
        private readonly IPrivilegedActionLogRepository _logRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PrivilegedActionLogService> _logger;

        public PrivilegedActionLogService(
            IPrivilegedActionLogRepository logRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PrivilegedActionLogService> logger)
        {
            _logRepository = logRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<PrivilegedActionLogDto> Logs, int TotalCount)> GetPagedLogsAsync(PrivilegedActionFilterDto filter)
        {
            var (logs, totalCount) = await _logRepository.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.StartDate,
                filter.EndDate,
                filter.ActionType,
                filter.UserId,
                filter.Severity,
                filter.EntityType);

            var logDtos = _mapper.Map<IEnumerable<PrivilegedActionLogDto>>(logs);
            return (logDtos, totalCount);
        }

        public async Task<IEnumerable<PrivilegedActionLogDto>> GetLogsByEntityAsync(string entityType, string entityId)
        {
            var logs = await _logRepository.GetByEntityAsync(entityType, entityId);
            return _mapper.Map<IEnumerable<PrivilegedActionLogDto>>(logs);
        }

        public async Task<IEnumerable<PrivilegedActionLogDto>> GetLogsByUserAsync(string userId, int limit = 100)
        {
            var logs = await _logRepository.GetByUserAsync(userId, limit);
            return _mapper.Map<IEnumerable<PrivilegedActionLogDto>>(logs);
        }

        public async Task<IEnumerable<PrivilegedActionLogDto>> GetCriticalActionsAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _logRepository.GetCriticalActionsAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<PrivilegedActionLogDto>>(logs);
        }

        public async Task LogActionAsync(
            string performedBy, 
            CreatePrivilegedActionLogDto request, 
            string? ipAddress = null, 
            string? userAgent = null, 
            string? requestPath = null)
        {
            var log = new PrivilegedActionLog
            {
                ActionType = request.ActionType,
                Severity = request.Severity,
                PerformedBy = performedBy,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                EntityName = request.EntityName,
                ActionDescription = request.ActionDescription,
                BeforeState = request.BeforeState,
                AfterState = request.AfterState,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                RequestPath = requestPath,
                Success = request.Success,
                ErrorMessage = request.ErrorMessage,
                Timestamp = DateTime.UtcNow,
                Metadata = request.Metadata
            };

            await _logRepository.LogActionAsync(log);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Privileged action logged: {ActionType} by {PerformedBy} on {EntityType}:{EntityId}",
                request.ActionType,
                performedBy,
                request.EntityType ?? "N/A",
                request.EntityId ?? "N/A");
        }
    }
}
