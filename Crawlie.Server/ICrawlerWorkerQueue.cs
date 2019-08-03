using System;
using System.Threading;

namespace Crawlie.Server
{
    public interface ICrawlerWorkerQueue
    {
        void Add(Uri uri);

        Uri Take(CancellationToken cancellationToken);
    }
}