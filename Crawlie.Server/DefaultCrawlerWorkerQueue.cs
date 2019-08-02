using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Crawlie.Server
{
    public class DefaultCrawlerWorkerQueue : ICrawlerWorkerQueue
    {
        private readonly BlockingCollection<string> _workerQueue = new BlockingCollection<string>();
        
        public void Add(string uriString)
        {
            _workerQueue.Add(uriString);
        }

        public string Take(CancellationToken cancellationToken)
        {
            try
            {
                return _workerQueue.Take(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}