using System;
using System.Net.Http;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Crawlie.Client
{
    public class CrawlerClient
    {
        private readonly HttpClient _httpClient;

        public CrawlerClient(
            HttpClient httpClient, 
            CrawlerClientConfiguration crawlerClientConfiguration)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = crawlerClientConfiguration.BaseAddress;
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

        public async Task<CrawlerJobResponse> GetJobRequestAsync(Uri targetUri)
        {
            var response =
                await _httpClient.GetAsync((string) QueryHelpers.AddQueryString("api/CrawlerJob", "jobId",
                    targetUri.ToString()));

            response.EnsureSuccessStatusCode();

            var serializedJobResponse = await response.Content.ReadAsStringAsync();
            var jobResponse = JsonConvert.DeserializeObject<CrawlerJobResponse>(serializedJobResponse);

            return jobResponse;
        }
    }
}