using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerWorkerTests
    {
        private readonly ServiceProvider _serviceProvider;

        public CrawlerWorkerTests(
            ITestOutputHelper outputHelper)
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddXUnit(outputHelper))
                .BuildServiceProvider();
        }
        
        [Fact]
        public async Task ProcessWorkItem_ValidItem_ShouldReturnCorrectLinks()
        {
            // Arrange

            var targetUri = new Uri("https://foobar.com");

            IDocumentFetcher documentFetcher = new TestDocumentFetcher();
            
            var workerQueueMock = new Mock<ICrawlerWorkerQueue>();

            var repositoryMock = new Mock<ISeedJobRepository>();
            repositoryMock
                .Setup(r =>
                    r.CompleteJob(
                        It.IsAny<Uri>(),
                        It.IsAny<List<Uri>>()));

            var crawlerEngineMock = new Mock<ICrawlerEngine>();
            var expectedUrls = new List<Uri>()
            {
                new Uri("https://foobar.com/"),
                new Uri("https://foobar.com/link1"),
                new Uri("https://foobar.com/link2"),
                new Uri("https://foobar.com/link3"),
                new Uri("https://foobar.com/link4"),
            };
            
            crawlerEngineMock
                .Setup(e =>
                    e.ProcessDocument(
                        It.IsAny<string>(),
                        targetUri.Host))
                .ReturnsAsync(expectedUrls);


            var logger = _serviceProvider.GetRequiredService<ILogger<CrawlerWorker>>();


            var cancellationTokenSource = new CancellationTokenSource(6000);
            var crawlerWorker = new CrawlerWorker(
                documentFetcher,
                repositoryMock.Object,
                crawlerEngineMock.Object, 
                workerQueueMock.Object, 
                logger, 
                cancellationTokenSource.Token);

            // Act
            await crawlerWorker.ProcessJob(targetUri);
            cancellationTokenSource.Cancel();

            // Assert
            repositoryMock
                .Verify(r => 
                    r.CompleteJob(
                        targetUri, 
                        expectedUrls));
        }

        [Fact]
        public async Task Start_JobIdInQueue_ShouldReturnCorrectLinks()
        {
            // Arrange
            var targetUri = new Uri("https://foobar.com");

            IDocumentFetcher documentFetcher = new TestDocumentFetcher();
            
            ICrawlerWorkerQueue workerQueue = new DefaultCrawlerWorkerQueue();
            workerQueue.Add(targetUri);
            
            var repositoryMock = new Mock<ISeedJobRepository>();
            repositoryMock
                .Setup(r =>
                    r.CompleteJob(
                        It.IsAny<Uri>(),
                        It.IsAny<List<Uri>>()));

            var crawlerEngineMock = new Mock<ICrawlerEngine>();
            var expectedUrls = new List<Uri>()
            {
                new Uri("https://foobar.com/"),
                new Uri("https://foobar.com/link1"),
                new Uri("https://foobar.com/link2"),
                new Uri("https://foobar.com/link3"),
                new Uri("https://foobar.com/link4"),
            };
            
            crawlerEngineMock
                .Setup(e =>
                    e.ProcessDocument(
                        It.IsAny<string>(),
                        targetUri.Host))
                .ReturnsAsync(expectedUrls);
            
            var cancellationTokenSource = new CancellationTokenSource(6000);
            
            var logger = _serviceProvider.GetRequiredService<ILogger<CrawlerWorker>>();

            var crawlerWorker = new CrawlerWorker(
                documentFetcher,
                repositoryMock.Object,
                crawlerEngineMock.Object, 
                workerQueue, 
                logger, 
                cancellationTokenSource.Token);

            // Act
            await crawlerWorker.StartAsync();
            cancellationTokenSource.Cancel();
            
            // Assert
            repositoryMock
                .Verify(r => 
                    r.CompleteJob(
                        targetUri, 
                        expectedUrls));
        }
    }
}