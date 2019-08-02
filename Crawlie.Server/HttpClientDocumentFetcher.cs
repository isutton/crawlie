using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class HttpClientDocumentFetcher : IDocumentFetcher
    {
        static HttpClientDocumentFetcher()
        {
            ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, errors) => true;
        }
        
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientDocumentFetcher(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetDocument(Uri uri)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var responseMessage = await httpClient.GetAsync(uri);

            var documentString = await responseMessage.Content.ReadAsStringAsync();

            return documentString;
        }
    }
}