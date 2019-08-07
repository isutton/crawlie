using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Crawlie.Server;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crawlie.Client.IntegrationTests
{
    public class FakeHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly HttpClient _httpClient;

        public FakeHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name = "")
        {
            return _httpClient;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ClientTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public ClientTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SubmitJobRequest_NewJob_ShouldReturnInProgress()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(_factory.CreateClient()));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            var jobRequest = new SeedJobRequest
            {
                Uri = new Uri("https://foobar.com")
            };

            // Act
            var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

            // Assert
            jobResponse.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
        }

        [Fact]
        public async Task SubmitJobRequest_UnfinishedJob_ShouldReturnInProgress()
        {
            var targetUri = new Uri("https://submitjobrequest-unfinishedjob.com");

            var httpClient = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var repository =
                            new ConcurrentCrawlerRepository();
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
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(httpClient));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            var jobRequest = new SeedJobRequest {Uri = targetUri};

            // Act
            var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

            // Assert
            jobResponse.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
        }

        [Fact]
        public async Task SubmitJobRequest_FinishedJob_ShouldReturnComplete()
        {
            var targetUri = new Uri("https://submitjobrequest-finishedjob.com");

            var httpClient = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var repository =
                            new ConcurrentCrawlerRepository();
                        repository.TryAddRange(new[]
                            {
                                new SeedJobStatus
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = SeedJobStatus.WorkerStatus.Complete
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(httpClient));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            var jobRequest = new SeedJobRequest
            {
                Uri = targetUri
            };

            // Act
            var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

            // Assert
            jobResponse.Status.Should().Be(SeedJobStatusResponse.JobStatus.Complete);
        }

        [Fact]
        public async Task GetJobRequest_UnknownJob_ShouldReturnNotFound()
        {
            var targetUri = new Uri("https://getjobrequest-unknownjob.com");

            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(_factory.CreateClient()));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            // Act
            var jobResponse = await crawlerClient.GetJobRequestAsync(targetUri, CancellationToken.None);

            // Assert
            jobResponse.Should().BeNull();
        }

        [Fact]
        public async Task GetJobRequest_UnfinishedJob_ShouldReturnInProgress()
        {
            var targetUri = new Uri("https://getjobrequest-unfinishedjob.com");
            var httpClient = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var repository =
                            new ConcurrentCrawlerRepository();
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
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(httpClient));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            // Act
            var jobResponse = await crawlerClient.GetJobRequestAsync(targetUri, CancellationToken.None);

            // Assert
            jobResponse.Status.Should().Be(SeedJobStatusResponse.JobStatus.InProgress);
        }


        [Fact]
        public async Task GetJobRequest_FinishedJob_ShouldReturnComplete()
        {
            var targetUri = new Uri("https://foobar.com");

            var httpClient = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var repository =
                            new ConcurrentCrawlerRepository();
                        repository.TryAddRange(new[]
                            {
                                new SeedJobStatus
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = SeedJobStatus.WorkerStatus.Complete
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"CrawlerClientOptions:BaseAddress", "https://localhost:5001"}
                })
                .Build();

            var crawlerClient =
                Program
                    .CreateHostBuilder(configuration)
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddSingleton<IHttpClientFactory>(provider =>
                            new FakeHttpClientFactory(httpClient));
                    })
                    .Build()
                    .Services
                    .GetService<CrawlerClient>();

            // Act
            var jobResponse = await crawlerClient.GetJobRequestAsync(targetUri, CancellationToken.None);

            // Assert
            jobResponse.Status.Should().Be(SeedJobStatusResponse.JobStatus.Complete);
        }
    }
}