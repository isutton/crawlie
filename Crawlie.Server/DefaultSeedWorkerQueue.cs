using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Crawlie.Server
{
    public class DefaultSeedWorkerQueue : ISeedWorkerQueue
    {
        private readonly BlockingCollection<Uri> _workerQueue = new BlockingCollection<Uri>();
        
        public void Add(Uri uri)
        {
            _workerQueue.Add(uri);
        }

        public Uri Take(CancellationToken cancellationToken)
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