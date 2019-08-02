using System;
using System.Threading.Tasks;

namespace Crawlie.Server.IntegrationTests
{
    public interface IDocumentFetcher
    {
        Task<string> GetDocument(Uri uri);
    }
}