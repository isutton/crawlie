using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Crawlie.Server.IntegrationTests
{
    public class CrawlerEngineTests
    {
        [Fact]
        public async Task ExtractLinks_ValidDocument_ReturnCorrectLinks()
        {
            // Arrange
            IDocumentFetcher documentFetcher = new TestDocumentFetcher();
            var engine = new CrawlerEngine();

            // Act
            var targetUri = new Uri("https://foobar.com");
            var result = await engine.ProcessDocument(await documentFetcher.GetDocument(targetUri), targetUri.Host);

            // Assert
            result.Should().BeEquivalentTo(new List<Uri>()
            {
                new Uri("https://foobar.com/link1"),
                new Uri("https://foobar.com/link2"),
                new Uri("https://foobar.com/link3"),
                new Uri("https://foobar.com/link4"),
            });
        }
    }
}