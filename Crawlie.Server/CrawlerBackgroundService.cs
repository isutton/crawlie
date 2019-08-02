using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        private readonly ICrawlerWorkerQueue _workerQueue;
        private readonly IServiceProvider _serviceProvider;
        private DefaultCrawlerSupervisor _supervisor;

        public CrawlerBackgroundService(
            ICrawlerWorkerQueue workerQueue,
            IServiceProvider serviceProvider)
        {
            _workerQueue = workerQueue;
            _serviceProvider = serviceProvider;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _supervisor = ActivatorUtilities.CreateInstance<DefaultCrawlerSupervisor>(_serviceProvider, cancellationToken);
            
            return _supervisor.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _supervisor.StopAsync(cancellationToken);
        }
    }
}