using System;
using System.IO;
using System.Threading.Tasks;

namespace Crawlie.Server.IntegrationTests
{
    public class TestDocumentFetcher : IDocumentFetcher
    {
        public Task<string> GetDocument(Uri uri)
        {
            var fileName = uri.ToString();
            fileName = fileName.Replace(':', '_');
            fileName = fileName.Replace('/', '_');
            
            var documentString = File.ReadAllText($"TestAssets/{fileName}.html");

            return Task.FromResult(documentString);
        }
    }
}