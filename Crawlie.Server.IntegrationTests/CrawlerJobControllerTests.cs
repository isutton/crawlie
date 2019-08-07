using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Crawlie.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerJobControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        public CrawlerJobControllerTests(
            ITestOutputHelper outputHelper,
            WebApplicationFactory<Startup> factory)
        {
            _outputHelper = outputHelper;
            _factory = factory;
        }

        private readonly ITestOutputHelper _outputHelper;
        private readonly WebApplicationFactory<Startup> _factory;

        [Fact]
        public async Task CreateJob_NewJob_ShouldReturnInProgress()
        {
            // Arrange
            var client = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddTransient<IDocumentFetcher, TestDocumentFetcher>();
                        services.Remove(new ServiceDescriptor(typeof(CrawlerBackgroundService),
                            typeof(CrawlerBackgroundService), ServiceLifetime.Transient));
                    });
                })
                .CreateClient();

            // Act
            var result = await client.PostAsJsonAsync("api/CrawlerJob", new SeedJobRequest
            {
                Uri = new Uri("https://foobar.com")
            });
            var responseContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<SeedJobStatusResponse>(responseContent);

            // Assert
            result.EnsureSuccessStatusCode();
            response.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
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
                        var loggerFactory = new LoggerFactory();
                        var repository =
                            new ConcurrentSeedJobRepository();
                        repository.TryAddRange(new[]
                            {
                                new SeedJobStatus
                                {
                                    Id = "https://foobar.com",
                                    Result = new List<Uri>(),
                                    Status = SeedJobStatus.WorkerStatus.Accepted
                                }
                            }
                        );
                        services.AddSingleton<ISeedJobRepository>(repository);
                        services.Remove(new ServiceDescriptor(typeof(CrawlerBackgroundService),
                            typeof(CrawlerBackgroundService)));
                    });
                })
                .CreateClient();

            // Act
            var result = await client.PostAsJsonAsync("api/CrawlerJob", new SeedJobRequest
            {
                Uri = new Uri("https://foobar.com")
            });
            var responseContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<SeedJobStatusResponse>(responseContent);

            // Assert
            result.EnsureSuccessStatusCode();
            response.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
        }

        [Fact]
        public async Task GetJobInfo_UnfinishedJob_ShouldReturnInProgress()
        {
            // Arrange
            const string jobId = "https://foobar.com/";
            var targetUri = new Uri(jobId);
            var client = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddTransient<IDocumentFetcher, TestDocumentFetcher>();
                        services.AddTransient<ICrawlerBackgroundService, NoopCrawlerBackgroundService>();
                        var loggerFactory = new LoggerFactory();
                        var repository =
                            new ConcurrentSeedJobRepository();

                        repository.TryAddRange(new[]
                            {
                                new SeedJobStatus
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = SeedJobStatus.WorkerStatus.Accepted
                                }
                            }
                        );
                        services.AddSingleton<ISeedJobRepository>(repository);
                        services.Remove(new ServiceDescriptor(typeof(CrawlerBackgroundService),
                            typeof(CrawlerBackgroundService)));
                    });
                })
                .CreateClient();

            // Act
            var result = await client.GetAsync(QueryHelpers.AddQueryString("api/CrawlerJob",
                new Dictionary<string, string>
                {
                    ["jobId"] = targetUri.ToString()
                })
            );
            var responseContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<SeedJobStatusResponse>(responseContent);

            // Assert
            result.EnsureSuccessStatusCode();
            response.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
        }
    }
}