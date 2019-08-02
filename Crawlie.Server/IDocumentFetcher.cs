using System;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public interface IDocumentFetcher
    {
        Task<string> GetDocument(Uri uri);
    }
}