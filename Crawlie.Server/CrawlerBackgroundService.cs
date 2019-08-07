using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crawlie.Server
{
    public interface ICrawlerBackgroundService : IHostedService {}
    
    public class NoopCrawlerBackgroundService : ICrawlerBackgroundService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
    
    
    public class CrawlerBackgroundService : ICrawlerBackgroundService
    {
        private readonly ILogger<CrawlerBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private DefaultSeedWorkerSupervisor _supervisor;

        public CrawlerBackgroundService(
            ILogger<CrawlerBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _supervisor = ActivatorUtilities.CreateInstance<DefaultSeedWorkerSupervisor>(_serviceProvider, cancellationToken);

            var executingTask = _supervisor.ExecuteAsync();

            _logger.LogInformation("Supervisor started");

            return executingTask.IsCompleted ? executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping supervisor.");
            
            await _supervisor.StopAsync(cancellationToken);
        }
    }
}