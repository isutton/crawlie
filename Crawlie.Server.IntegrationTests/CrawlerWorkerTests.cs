using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerWorkerTests
    {
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
                    r.CompleteJobAsync(
                        It.IsAny<string>(),
                        It.IsAny<List<Uri>>()))
                .Returns(Task.CompletedTask);

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


            var crawlerWorker = new CrawlerWorker(
                documentFetcher,
                repositoryMock.Object,
                crawlerEngineMock.Object);

            // Act
            await crawlerWorker.ProcessJob(targetUri.ToString());

            // Assert
            repositoryMock
                .Verify(r => 
                    r.CompleteJobAsync(
                        targetUri.ToString(), 
                        expectedUrls));
        }
    }
}