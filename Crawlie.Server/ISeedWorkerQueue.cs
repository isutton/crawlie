using System;
using System.Threading;

namespace Crawlie.Server
{
    public interface ISeedWorkerQueue
    {
        void Add(Uri uri);

        Uri Take(CancellationToken cancellationToken);
    }
}