using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Crawlie.Server;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Crawlie.Client.IntegrationTests
{
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
            using (var httpClient = _factory.CreateClient())
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);
                var jobRequest = new CrawlerJobRequest()
                {
                    Uri = new Uri("https://foobar.com")
                };

                // Act
                var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

                // Assert
                jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
            }
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
                                new CrawlerJobInfo
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = CrawlerJobInfo.WorkerStatus.Accepted
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            using (httpClient)
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);
                var jobRequest = new CrawlerJobRequest {Uri = targetUri};

                // Act
                var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

                // Assert
                jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
            }
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
                                new CrawlerJobInfo
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = CrawlerJobInfo.WorkerStatus.Complete
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            using (httpClient)
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);
                var jobRequest = new CrawlerJobRequest()
                {
                    Uri = targetUri
                };

                // Act
                var jobResponse = await crawlerClient.SubmitJobRequest(jobRequest);

                // Assert
                jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.Complete);
            }
        }

        [Fact]
        public void GetJobRequest_UnknownJob_ShouldReturnNotFound()
        {
            var targetUri = new Uri("https://getjobrequest-unknownjob.com");

            using (var httpClient = _factory.CreateClient())
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);
                Func<Task> act = async () => await crawlerClient.GetJobRequestAsync(targetUri);

                // Act
                act.Should().Throw<HttpRequestException>();
            }
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
                                new CrawlerJobInfo
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = CrawlerJobInfo.WorkerStatus.Accepted
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            using (httpClient)
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);

                // Act
                var jobResponse = await crawlerClient.GetJobRequestAsync(targetUri);

                // Assert
                jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
            }
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
                                new CrawlerJobInfo
                                {
                                    Id = targetUri.ToString(),
                                    Result = new List<Uri>(),
                                    Status = CrawlerJobInfo.WorkerStatus.Complete
                                }
                            }
                        );
                        services.AddSingleton<ICrawlerRepository>(repository);
                    });
                })
                .CreateClient();

            using (httpClient)
            {
                // Arrange
                var crawlerClientConfiguration = new CrawlerClientConfiguration()
                {
                    BaseAddress = new Uri("https://localhost:5001")
                };
                var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);

                // Act
                var jobResponse = await crawlerClient.GetJobRequestAsync(targetUri);

                // Assert
                jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.Complete);
            }
        }
    }
}