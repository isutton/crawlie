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

        private async Task<List<Uri>> GetDocumentLinksAsync(Uri targetUri, CancellationToken cancellationToken)
        {
            
            cancellationToken.ThrowIfCancellationRequested();

            var jobStatus = await _crawlerClient.GetJobRequestAsync(targetUri, cancellationToken);

            if (jobStatus == null)
            {
                var jobRequest = new SeedJobRequest
                {
                    Uri = targetUri
                };

                jobStatus = await _crawlerClient.SubmitJobRequest(jobRequest);

                while (!cancellationToken.IsCancellationRequested &&
                       jobStatus.Status != CrawlerJobResponse.JobStatus.Complete)
                {
                    await Task.Delay(500, cancellationToken);
                    
                    jobStatus = await _crawlerClient.GetJobRequestAsync(new Uri(jobStatus.Id),
                        cancellationToken);
                }
            }

            var documentLinks = jobStatus.Result;

            return documentLinks;
        }
        
        public async Task Run()
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(60000);

                var documentLinks = await GetDocumentLinksAsync(new Uri(_runnerOptions.Value.Url), cancellationTokenSource.Token);
                
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