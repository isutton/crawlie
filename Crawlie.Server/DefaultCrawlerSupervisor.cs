using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Crawlie.Server
{
    public class DefaultCrawlerSupervisor : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private const uint WorkerPoolSize = 1;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private List<Task> _workers;

        public DefaultCrawlerSupervisor(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public Task StartAsync()
        {
            _workers = new List<Task>((int) WorkerPoolSize);
            
            for (var i = 0; i < WorkerPoolSize; i++)
            {
                var f = ActivatorUtilities.CreateInstance<CrawlerWorker>(_serviceProvider, _cancellationTokenSource.Token);

                var workerTask = Task.Run(f.Start, _cancellationTokenSource.Token);
                _workers.Add(workerTask);
            }

            return Task.WhenAll(_workers);
        }

        public Task StopAsync(in CancellationToken cancellationToken)
        {
            // Shortcut here, until there's cancellationToken handling in the
            // CrawlerWorker itself.
            _cancellationTokenSource.Cancel();
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}