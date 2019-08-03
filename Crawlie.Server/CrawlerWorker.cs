using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Crawlie.Server
{
    public class CrawlerWorker
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ICrawlerEngine _crawlerEngine;
        private readonly IDocumentFetcher _documentFetcher;
        private readonly ILogger<CrawlerWorker> _logger;
        private readonly ICrawlerRepository _repository;
        private readonly ICrawlerWorkerQueue _workerQueue;

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

        public async Task ProcessJob(Uri targetUri)
        {
            try
            {
                _logger.LogDebug($"Downloading document from {targetUri}.");
                
                var documentString = await _documentFetcher.GetDocument(targetUri);

                _logger.LogDebug($"Extracting document links.");
                
                var documentLinks = await _crawlerEngine.ProcessDocument(documentString, targetUri.Host);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    var documentLinksJsonString = JsonConvert.SerializeObject(documentLinks, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented
                    });
                    
                    _logger.LogTrace($"Extracted links: {documentLinksJsonString}");
                }

                _repository.CompleteJob(targetUri, documentLinks);
            }
            catch (Exception e)
            {
                throw new NotImplementedException("Work in progress", e);
            }
        }

        public async Task StartAsync()
        {
            // Note: if MainLoop could be configured, here would be the place
            //       to inject configuration coming from the application.
            await MainLoop();
        }

        private async Task MainLoop()
        {
            while (true)
            {
                var jobId = _workerQueue.Take(_cancellationToken);

                // _cancellationToken has been cancelled.
                if (jobId == null) return;

                _logger.LogDebug($"Processing {jobId}.");

                await ProcessJob(jobId);

                _logger.LogDebug($"Processed {jobId}.");
            }
        }
    }
}