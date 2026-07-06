using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Dialysis.Server.Domain.Services
{
    public interface ILogSettingsProvider
    {
        Task<LogSettings> GetSettingsAsync();
        void Refresh();
    }

    public class LogSettingsProvider : ILogSettingsProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private LogSettings? _cachedSettings;
        private readonly object _lock = new();
        private DateTime _lastUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public LogSettingsProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<LogSettings> GetSettingsAsync()
        {
            if (_cachedSettings != null && DateTime.UtcNow - _lastUpdate < _cacheDuration)
            {
                return _cachedSettings;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var settings = await dbContext.Set<LogSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new LogSettings();
                dbContext.Set<LogSettings>().Add(settings);
                await dbContext.SaveChangesAsync();
            }

            lock (_lock)
            {
                _cachedSettings = settings;
                _lastUpdate = DateTime.UtcNow;
            }

            return settings;
        }

        public void Refresh()
        {
            lock (_lock)
            {
                _cachedSettings = null;
            }
        }
    }
}
