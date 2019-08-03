using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Crawlie.Client
{
    public class CrawlerClient : IDisposable
    {
        private readonly ILogger<CrawlerClient> _logger;
        private readonly HttpClient _httpClient;

        public CrawlerClient(
            HttpClient httpClient,
            ILogger<CrawlerClient> logger,
            IOptions<CrawlerClientOptions> crawlerClientConfiguration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = crawlerClientConfiguration.Value.BaseAddress;
            _httpClient.Timeout = TimeSpan.FromSeconds(2.0);
        }

        public async Task<CrawlerJobResponse> SubmitJobRequest(CrawlerJobRequest jobRequest)
        {
            var serializedJobRequestContent = JsonConvert.SerializeObject(jobRequest);
            var jobRequestContent = new StringContent(
                serializedJobRequestContent, 
                System.Text.Encoding.UTF8, 
                "application/json");
            var response = await _httpClient.PostAsync("api/CrawlerJob", jobRequestContent);

            // FIXME: Return proper error to the caller.
            response.EnsureSuccessStatusCode();

            var serializedJobResponse = await response.Content.ReadAsStringAsync();
            var jobResponse = JsonConvert.DeserializeObject<CrawlerJobResponse>(serializedJobResponse);

            return jobResponse;
        }

        public async Task<CrawlerJobResponse> GetJobRequestAsync(Uri targetUri, CancellationToken cancellationToken)
        {
            try
            {
                var response =
                    await _httpClient.GetAsync(
                        QueryHelpers.AddQueryString(
                            "api/CrawlerJob", "jobId", targetUri.ToString()),
                        cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var serializedJobResponse = await response.Content.ReadAsStringAsync();
                var jobResponse = JsonConvert.DeserializeObject<CrawlerJobResponse>(serializedJobResponse);

                return jobResponse;
            }
            catch (TaskCanceledException e)
            {
                _logger.LogError(e, e.Message);
                throw new CrawlerTimeoutException();
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}