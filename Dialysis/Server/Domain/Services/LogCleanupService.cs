using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Log Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up logs.");
                }

                // Wait for 24 hours before next cleanup
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task DoCleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settingsProvider = scope.ServiceProvider.GetRequiredService<ILogSettingsProvider>();
            
            var settings = await settingsProvider.GetSettingsAsync();
            if (settings == null || settings.RetentionDays <= 0) return;

            var cutoffDate = DateTime.Now.AddDays(-settings.RetentionDays);
            _logger.LogInformation("Cleaning up logs older than {CutoffDate}", cutoffDate);

            // Cleanup HttpLogs
            var oldHttpLogs = await dbContext.Set<HttpLog>()
                .Where(l => l.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();

            // Cleanup ActionLogs
            var oldActionLogs = await dbContext.Set<ActionLog>()
                .Where(l => l.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Cleaned up {HttpCount} HTTP logs and {ActionCount} Action logs.", oldHttpLogs, oldActionLogs);
        }
    }
}
