using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerServiceTests
    {
        [Fact]
        public async Task HandleJobRequest_FinishedJob_ReturnDoneAndResult()
        {
            // Arrange
            var resultList = new List<Uri> {new Uri("https://www.redhat.com")};
            var loggerFactory = new LoggerFactory();
            var repository = new ConcurrentCrawlerRepository();
            repository.TryAddRange(new[]
            {
                new SeedJobStatus
                {
                    Id = "https://www.redhat.com/en/topics/cloud",
                    Status = SeedJobStatus.WorkerStatus.Complete,
                    Result = resultList
                }
            });
            var workerQueue = new DefaultCrawlerWorkerQueue();
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueue);
            var jobRequest = new SeedJobRequest
            {
                Uri = new Uri("https://www.redhat.com/en/topics/cloud")
            };

            // Act
            var jobResponse = await crawlerService.HandleJobRequest(jobRequest);

            // Assert
            jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.Complete);
            jobResponse.Result.Should().BeEquivalentTo(resultList);
        }

        [Fact]
        public async Task HandleJobRequest_NewJob_ReturnInProgress()
        {
            // Arrange
            const string jobId = "https://www.redhat.com/en/topics/cloud";

            var loggerFactory = new LoggerFactory();
            var repository = new ConcurrentCrawlerRepository();
            var workerQueueMock = new Mock<ICrawlerWorkerQueue>();
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueueMock.Object);
            var targetUri = new Uri(jobId);
            var jobRequest = new SeedJobRequest
            {
                Uri = targetUri
            };

            // Act
            var jobResponse = await crawlerService.HandleJobRequest(jobRequest);


            // Assert
            jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
            workerQueueMock.Verify(w => w.Add(targetUri));
        }

        [Fact]
        public async Task HandleJobRequest_UnfinishedJob_ReturnInProgress()
        {
            // Arrange
            const string jobId = "https://www.redhat.com/en/topics/cloud";
            var loggerFactory = new LoggerFactory();
            var repository = new ConcurrentCrawlerRepository();
            repository.TryAddRange(new[]
            {
                new SeedJobStatus
                {
                    Id = jobId,
                    Status = SeedJobStatus.WorkerStatus.Accepted
                }
            });
            var targetUri = new Uri(jobId);
            var workerQueue = new Mock<ICrawlerWorkerQueue>();
            workerQueue.Setup(w => w.Add(targetUri));
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueue.Object);
            var jobRequest = new SeedJobRequest
            {
                Uri = targetUri
            };

            // Act
            var jobResponse = await crawlerService.HandleJobRequest(jobRequest);


            // Assert
            jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
        }
    }
}