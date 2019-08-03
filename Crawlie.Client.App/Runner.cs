using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Crawlie.Client
{
    internal class Runner
    {
        private readonly ILogger<Runner> _logger;
        private readonly IOptions<RunnerOptions> _runnerOptions;
        private readonly CrawlerClient _crawlerClient;

        public Runner(
            ILogger<Runner> logger,
            IOptions<RunnerOptions> runnerOptions,
            CrawlerClient crawlerClient)
        {
            _logger = logger;
            _runnerOptions = runnerOptions;
            _crawlerClient = crawlerClient;
        }

        public async Task Run()
        {
            var targetUri = new Uri(_runnerOptions.Value.Url);

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(10000);
                
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var jobStatus = await _crawlerClient.GetJobRequestAsync(targetUri, cancellationTokenSource.Token);

                if (jobStatus == null)
                {
                    var jobRequest = new CrawlerJobRequest
                    {
                        Uri = targetUri
                    };

                    jobStatus = await _crawlerClient.SubmitJobRequest(jobRequest);

                    while (!cancellationTokenSource.IsCancellationRequested &&
                           jobStatus.Status != CrawlerJobResponse.JobStatus.Complete)
                    {
                        await Task.Delay(500, cancellationTokenSource.Token);
                        jobStatus = await _crawlerClient.GetJobRequestAsync(new Uri(jobStatus.Id), cancellationTokenSource.Token);
                    }
                }

                Console.WriteLine(JsonConvert.SerializeObject(jobStatus, Formatting.Indented));
            }
            catch (HttpRequestException e)
            {
                throw new NotImplementedException("Work in progress.", e);
            }
            catch (OperationCanceledException e)
            {
                throw new NotImplementedException("Work in progress.", e);
            }
            catch (CrawlerTimeoutException e)
            {
                _logger.LogWarning("A timeout happened while contacting the server.");
            }
        }
    }
}