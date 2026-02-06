using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Services
{
    public class DynamicGroupUpdateProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DynamicGroupUpdateProcessor> _logger;
        private readonly TimeSpan _updateInterval;

        public DynamicGroupUpdateProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<DynamicGroupUpdateProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Read update interval from configuration, default to 15 minutes
            var intervalMinutes = configuration.GetValue<int?>("DynamicGroupSettings:UpdateIntervalMinutes") ?? 15;
            _updateInterval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dynamic Group Update Processor started. Update interval: {Interval}", _updateInterval);

            // Wait for initial delay before first run
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var groupRepository = scope.ServiceProvider.GetRequiredService<IRepository<ContactGroup>>();
                    var dynamicGroupService = scope.ServiceProvider.GetRequiredService<IDynamicGroupEvaluationService>();

                    // Get all dynamic groups
                    var dynamicGroups = await groupRepository.FindAsync(g => 
                        g.IsDynamic && !g.IsDeleted);

                    if (!dynamicGroups.Any())
                    {
                        _logger.LogDebug("No dynamic groups found to update");
                    }
                    else
                    {
                        _logger.LogInformation("Found {Count} dynamic groups to update", dynamicGroups.Count());

                        // Group by user to update all groups for each user
                        var groupsByUser = dynamicGroups.GroupBy(g => g.UserId);

                        foreach (var userGroups in groupsByUser)
                        {
                            try
                            {
                                await dynamicGroupService.UpdateAllDynamicGroupsAsync(userGroups.Key);
                                _logger.LogInformation("Updated {Count} dynamic groups for user {UserId}", 
                                    userGroups.Count(), userGroups.Key);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error updating dynamic groups for user {UserId}", userGroups.Key);
                            }
                        }
                    }

                    // Wait for the configured interval before next update
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in dynamic group update processor");
                    // Wait a bit longer on error before retrying
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Dynamic Group Update Processor stopped");
        }
    }
}
