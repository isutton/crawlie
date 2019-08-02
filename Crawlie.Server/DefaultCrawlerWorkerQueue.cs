using System.Collections.Concurrent;

namespace Crawlie.Server
{
    public class DefaultCrawlerWorkerQueue : ICrawlerWorkerQueue
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