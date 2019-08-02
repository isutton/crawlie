using System.Collections.Concurrent;

namespace Crawlie.Server.IntegrationTests
{
    class DefaultCrawlerWorkerQueue : ICrawlerWorkerQueue
    {
        private readonly BlockingCollection<string> _workerQueue = new BlockingCollection<string>();
        
        public void Add(string uriString)
        {
            _workerQueue.Add(uriString);
        }

        public string Take()
        {
            return _workerQueue.Take();
        }
    }
}