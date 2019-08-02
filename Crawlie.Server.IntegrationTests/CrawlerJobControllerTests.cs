using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerJobControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly WebApplicationFactory<Startup> _factory;

        public CrawlerJobControllerTests(
            ITestOutputHelper outputHelper,
            WebApplicationFactory<Startup> factory)
        {
            _outputHelper = outputHelper;
            _factory = factory;
        }

        [Fact]
        public async Task CreateJob_NewJob_ShouldReturnInProgress()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Act
            var result = await client.PostAsJsonAsync("api/CrawlerJob", new CrawlerJobRequest()
            {
                Uri = new Uri("https://foobar.com")
            });
            var responseContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<CrawlerJobResponse>(responseContent);

            // Assert
            result.EnsureSuccessStatusCode();
            response.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
        }

        [Fact]
        public async Task CreateJob_UnfinishedExistingJob_ShouldReturnInProgress()
        {
            // Arrange
            var client = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var repository = ConcurrentCrawlerRepository.NewWithExistingJobs(
                            new CrawlerJobInfo()
                            {
                                Id = "https://foobar.com",
                                Result = new List<Uri>(),
                                Status = CrawlerJobInfo.WorkerStatus.Accepted
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();
            
            // Act
            var result = await client.PostAsJsonAsync("api/CrawlerJob", new CrawlerJobRequest()
            {
                Uri = new Uri("https://foobar.com")
            });
            var responseContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<CrawlerJobResponse>(responseContent);

            // Assert
            result.EnsureSuccessStatusCode();
            response.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
        }
    }
}