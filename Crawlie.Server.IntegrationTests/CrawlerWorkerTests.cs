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

            var repositoryMock = new Mock<ICrawlerRepository>();
            repositoryMock
                .Setup(r =>
                    r.CompleteJob(
                        It.IsAny<string>(),
                        It.IsAny<List<Uri>>()));

            var crawlerEngineMock = new Mock<ICrawlerEngine>();
            var expectedUrls = new List<Uri>()
            {
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
            
            var crawlerWorker = new CrawlerWorker(
                documentFetcher,
                repositoryMock.Object,
                crawlerEngineMock.Object, 
                workerQueueMock.Object, 
                logger, 
                new CancellationTokenSource(2000).Token);

            // Act
            await crawlerWorker.ProcessJob(targetUri.ToString());

            // Assert
            repositoryMock
                .Verify(r => 
                    r.CompleteJob(
                        targetUri.ToString(), 
                        expectedUrls));
        }

        [Fact]
        public async Task Start_JobIdInQueue_ShouldReturnCorrectLinks()
        {
            // Arrange
            var targetUri = new Uri("https://foobar.com");

            IDocumentFetcher documentFetcher = new TestDocumentFetcher();
            
            ICrawlerWorkerQueue workerQueue = new DefaultCrawlerWorkerQueue();
            workerQueue.Add(targetUri.ToString());
            
            var repositoryMock = new Mock<ICrawlerRepository>();
            repositoryMock
                .Setup(r =>
                    r.CompleteJob(
                        It.IsAny<string>(),
                        It.IsAny<List<Uri>>()));

            var crawlerEngineMock = new Mock<ICrawlerEngine>();
            var expectedUrls = new List<Uri>()
            {
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
            
            var tokenSource = new CancellationTokenSource(2000);
            
            var logger = _serviceProvider.GetRequiredService<ILogger<CrawlerWorker>>();

            var crawlerWorker = new CrawlerWorker(
                documentFetcher,
                repositoryMock.Object,
                crawlerEngineMock.Object, 
                workerQueue, 
                logger, 
                tokenSource.Token);

            // Act
            await crawlerWorker.Start();
            tokenSource.Cancel();
            
            // Assert
            repositoryMock
                .Verify(r => 
                    r.CompleteJob(
                        targetUri.ToString(), 
                        expectedUrls));
        }
    }
}