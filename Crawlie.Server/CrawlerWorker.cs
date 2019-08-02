using System;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class CrawlerWorker
    {
        private readonly IDocumentFetcher _documentFetcher;
        private readonly ICrawlerRepository _repository;
        private readonly ICrawlerEngine _crawlerEngine;

        public CrawlerWorker(
            IDocumentFetcher documentFetcher,
            ICrawlerRepository repository,
            ICrawlerEngine crawlerEngine)
        {
            _documentFetcher = documentFetcher;
            _repository = repository;
            _crawlerEngine = crawlerEngine;
        }

        public async Task ProcessJob(string jobId)
        {
            var targetUri = new Uri(jobId);
            var documentString = await _documentFetcher.GetDocument(targetUri);
            var documentLinks = await _crawlerEngine.ProcessDocument(documentString, targetUri.Host);
            await _repository.CompleteJobAsync(jobId, documentLinks);
        }
    }
}