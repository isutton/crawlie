using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crawlie.Client
{
    internal class Runner
    {
        private readonly CrawlerClient _crawlerClient;
        private readonly ISiteMapFormatter _siteMapFormatter;
        private readonly ILogger<Runner> _logger;
        private readonly IOptions<RunnerOptions> _runnerOptions;

        public Runner(
            ISiteMapFormatter siteMapFormatter,
            ILogger<Runner> logger,
            IOptions<RunnerOptions> runnerOptions,
            CrawlerClient crawlerClient)
        {
            _siteMapFormatter = siteMapFormatter;
            _logger = logger;
            _runnerOptions = runnerOptions;
            _crawlerClient = crawlerClient;
        }

        private async Task<List<Uri>> GetDocumentLinksAsync(Uri targetUri)
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

                while (!cancellationTokenSource.Token.IsCancellationRequested &&
                       jobStatus.Status != CrawlerJobResponse.JobStatus.Complete)
                {
                    await Task.Delay(500, cancellationTokenSource.Token);
                    
                    jobStatus = await _crawlerClient.GetJobRequestAsync(new Uri(jobStatus.Id),
                        cancellationTokenSource.Token);
                }
            }

            var documentLinks = jobStatus.Result;

            return documentLinks;
        }
        
        public async Task Run()
        {
            try
            {
                var documentLinks = await GetDocumentLinksAsync(new Uri(_runnerOptions.Value.Url));
                
                var siteMapString = _siteMapFormatter.Format(documentLinks);

                Console.WriteLine(siteMapString);
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