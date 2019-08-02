using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crawlie.Server
{
    public class CrawlerWorker
    {
        private readonly IDocumentFetcher _documentFetcher;
        private readonly ICrawlerRepository _repository;
        private readonly ICrawlerEngine _crawlerEngine;
        private readonly ICrawlerWorkerQueue _workerQueue;
        private readonly ILogger<CrawlerWorker> _logger;
        private readonly CancellationToken _cancellationToken;

        public CrawlerWorker(
            IDocumentFetcher documentFetcher,
            ICrawlerRepository repository,
            ICrawlerEngine crawlerEngine,
            ICrawlerWorkerQueue workerQueue,
            ILogger<CrawlerWorker> logger,
            CancellationToken cancellationToken)
        {
            _documentFetcher = documentFetcher;
            _repository = repository;
            _crawlerEngine = crawlerEngine;
            _workerQueue = workerQueue;
            _logger = logger;
            _cancellationToken = cancellationToken;
        }

        public async Task ProcessJob(string jobId)
        {
            try
            {
                var targetUri = new Uri(jobId);
                var documentString = await _documentFetcher.GetDocument(targetUri);
                var documentLinks = await _crawlerEngine.ProcessDocument(documentString, targetUri.Host);
                _repository.CompleteJob(jobId, documentLinks);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task Start()
        {
            while (true)
            {
                var jobId = _workerQueue.Take(_cancellationToken);
                
                // In the case _cancellationToken is cancelled, jobId is going
                // to be null, so just bail out.
                if (jobId == null)
                {
                    return;
                }

                _logger.LogDebug($"Processing {jobId}...");
                await ProcessJob(jobId);
                _logger.LogDebug($"Processed {jobId}.");
            }
        }
    }
}