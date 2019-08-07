using System;
using System.Collections.Concurrent;
using System.Linq;
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
        private readonly ISeedJobRepository _repository;
        private readonly ISeedWorkerQueue _workerQueue;

        public CrawlerWorker(
            IDocumentFetcher documentFetcher,
            ISeedJobRepository repository,
            ICrawlerEngine crawlerEngine,
            ISeedWorkerQueue workerQueue,
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

        private static TimeSpan GetRandomDelay()
        {
            var random = new Random();
            return TimeSpan.FromSeconds(random.NextDouble() * 1.65);
        }

        public static bool CanProcessJobInternal(Uri targetUri)
        {
            return targetUri.Scheme == "http" || targetUri.Scheme == "https";
        }
        
        // This is a recursive method that executes itself for each URL
        // extracted from the document found in targetUri. seenUrls is
        // a ConcurrentDictionary that is used to collect the results
        // from all Tasks.
        private async Task ProcessJobInternal(Uri targetUri, ConcurrentDictionary<Uri, object> seenUrls, TimeSpan delay,
            uint recursionLevel)
        {
            if (recursionLevel == 0) return;
            
            if (!CanProcessJobInternal(targetUri)) return;
            
            await Task.Delay(delay);

            if (!seenUrls.TryAdd(targetUri, default)) return;

            try
            {
                _logger.LogDebug($"Downloading document from {targetUri}.");

                var documentString = await _documentFetcher.GetDocument(targetUri);

                _logger.LogDebug("Extracting document links.");

                var documentLinks = await _crawlerEngine.ProcessDocument(documentString, targetUri.Host);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    var documentLinksJsonString = JsonConvert.SerializeObject(documentLinks, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    });

                    _logger.LogTrace($"Extracted links: {documentLinksJsonString}");
                }

                if (documentLinks.Count > 0)
                {
                    var tasks = documentLinks
                        .Where(uri =>
                        {
                            var isUnknown = !seenUrls.ContainsKey(uri);
                            var isSelfReference = uri.Equals(targetUri);
                            return isUnknown && !isSelfReference;
                        })
                        .Select(uri => ProcessJobInternal(uri, seenUrls, GetRandomDelay(), recursionLevel - 1));

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception e)
            {
                throw new NotImplementedException("Work in progress", e);
            }
        }

        public async Task ProcessJob(Uri targetUri)
        {
            var seenUrls = new ConcurrentDictionary<Uri, object>();
            await ProcessJobInternal(targetUri, seenUrls, TimeSpan.Zero, 5);
            var sortedSeenUrls = seenUrls.Keys.OrderBy(uri => uri.ToString()).ToList();
            _repository.CompleteJob(targetUri, sortedSeenUrls);
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