using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class TempFileCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<TempFileCleanupService> _logger;
        private Timer _timer;

        public TempFileCleanupService(ILogger<TempFileCleanupService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporary File Cleanup Service running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "SEBIZGames");
            if (!Directory.Exists(tempPath))
            {
                return;
            }

            foreach (var dir in Directory.GetDirectories(tempPath))
            {
                var lastAccessTime = Directory.GetLastAccessTime(dir);
                if (lastAccessTime < DateTime.Now.AddHours(-24))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting temporary directory: {dir}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporary File Cleanup Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
