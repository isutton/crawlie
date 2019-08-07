using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crawlie.Server
{
    public class DefaultSeedWorkerSupervisor : IDisposable
    {
        private Task ExecutingTask { get; set; }
        private const uint WorkerPoolSize = 1;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<DefaultSeedWorkerSupervisor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<Task> _workers;

        public DefaultSeedWorkerSupervisor(
            ILogger<DefaultSeedWorkerSupervisor> logger,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _workers = new List<Task>((int) WorkerPoolSize);
        }
        
        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        public Task ExecuteAsync()
        {
            _logger.LogInformation("Creating workers");

            foreach (var i in Enumerable.Range(1, (int) WorkerPoolSize))
            {
                var worker =
                    ActivatorUtilities.CreateInstance<SeedWorker>(_serviceProvider, _cancellationTokenSource.Token);
                _workers.Add(Task.Run(worker.StartAsync, _cancellationTokenSource.Token));
            }

            _logger.LogInformation("Workers created");

            ExecutingTask = Task.WhenAll(_workers);

            return ExecutingTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (ExecutingTask == null) return;

            // Shortcut here, until there's cancellationToken handling in the
            // CrawlerWorker itself.
            _cancellationTokenSource.Cancel();

            await Task.WhenAny(ExecutingTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Supervisor stopped.");
        }
    }
}