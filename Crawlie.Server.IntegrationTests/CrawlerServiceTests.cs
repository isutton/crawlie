using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
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
            var repository = ConcurrentCrawlerRepository.NewWithExistingJobs(new CrawlerJobInfo
            {
                Id = "https://www.redhat.com/en/topics/cloud",
                Status = CrawlerJobInfo.WorkerStatus.Complete,
                Result = resultList
            });
            var workerQueue = new DefaultCrawlerWorkerQueue();
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueue);
            var jobRequest = new CrawlerJobRequest
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
            
            var repository = new ConcurrentCrawlerRepository();
            var workerQueueMock = new Mock<ICrawlerWorkerQueue>();
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueueMock.Object);
            var jobRequest = new CrawlerJobRequest
            {
                Uri = new Uri(jobId)
            };

            // Act
            var jobResponse = await crawlerService.HandleJobRequest(jobRequest);


            // Assert
            jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
            workerQueueMock.Verify(w => w.Add(jobId));
        }

        [Fact]
        public async Task HandleJobRequest_UnfinishedJob_ReturnInProgress()
        {
            // Arrange
            const string jobId = "https://www.redhat.com/en/topics/cloud";
            
            var repository = ConcurrentCrawlerRepository.NewWithExistingJobs(new CrawlerJobInfo
            {
                Id = jobId,
                Status = CrawlerJobInfo.WorkerStatus.Accepted
            });
            var workerQueue = new Mock<ICrawlerWorkerQueue>();
            workerQueue.Setup(w => w.Add(jobId));
            var crawlerService = new DefaultCrawlerJobService(repository, workerQueue.Object);
            var jobRequest = new CrawlerJobRequest
            {
                Uri = new Uri(jobId)
            };

            // Act
            var jobResponse = await crawlerService.HandleJobRequest(jobRequest);


            // Assert
            jobResponse.Status.Should().Be(CrawlerJobResponse.JobStatus.InProgress);
        }
    }
}