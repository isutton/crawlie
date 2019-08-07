using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Crawlie.Server.IntegrationTests
{
    public class SeedGraphRepositoryTests
    {
        [Fact]
        public async Task GetConnectedVerticesAsync_WithChildren_ShouldReturnCorrectList()
        {
            // Arrange
            var targetUri = new Vertex {Uri = new Uri("https://foobar.com")};
            var link1 = new Vertex {Uri = new Uri("https://foobar.com/link1")};
            var link2 = new Vertex {Uri = new Uri("https://foobar.com/link2")};
            var link3 = new Vertex {Uri = new Uri("https://foobar.com/link3")};

            var seedGraphRepository = new SeedGraphRepository();

            seedGraphRepository.AddVertices(targetUri, link1, link2, link3);
            seedGraphRepository.AddEdges(
                new Edge {From = targetUri, To = link1},
                new Edge {From = targetUri, To = link2},
                new Edge {From = targetUri, To = link3}
            );

            // Act
            var targetUriResult = await seedGraphRepository.GetConnectedVerticesAsync(targetUri);
            var link1Result = await seedGraphRepository.GetConnectedVerticesAsync(link1);
            var link2Result = await seedGraphRepository.GetConnectedVerticesAsync(link2);
            var link3Result = await seedGraphRepository.GetConnectedVerticesAsync(link3);

            // Assert
            targetUriResult.Should().BeEquivalentTo(new List<Vertex>
            {
                targetUri,
                link1,
                link2,
                link3
            });
            link1Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link1
            });
            link2Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link2
            });
            link3Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link3
            });
        }

        [Fact]
        public async Task GetConnectedVerticesAsync_WithGrandChildren_ShouldReturnCorrectList()
        {
            // Arrange
            var targetUri = new Vertex {Uri = new Uri("https://foobar.com")};
            var link1 = new Vertex {Uri = new Uri("https://foobar.com/link1")};
            var link2 = new Vertex {Uri = new Uri("https://foobar.com/link2")};
            var link3 = new Vertex {Uri = new Uri("https://foobar.com/link3")};

            var seedGraphRepository = new SeedGraphRepository();

            seedGraphRepository.AddVertices(targetUri, link1, link2, link3);
            seedGraphRepository.AddEdges(
                new Edge {From = targetUri, To = link1},
                new Edge {From = link1, To = link2},
                new Edge {From = link1, To = link3}
            );

            // Act
            var targetUriResult = await seedGraphRepository.GetConnectedVerticesAsync(targetUri);
            var link1Result = await seedGraphRepository.GetConnectedVerticesAsync(link1);
            var link2Result = await seedGraphRepository.GetConnectedVerticesAsync(link2);
            var link3Result = await seedGraphRepository.GetConnectedVerticesAsync(link3);

            // Assert
            targetUriResult.Should().BeEquivalentTo(new List<Vertex>
            {
                targetUri,
                link1,
                link2,
                link3
            });
            link1Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link1,
                link2,
                link3
            });
            link2Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link2
            });
            link3Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link3
            });
        }

        [Fact]
        public async Task GetConnectedVerticesAsync_WithGrandGrandChildren_ShouldReturnCorrectList()
        {
            // Arrange
            var targetUri = new Vertex {Uri = new Uri("https://foobar.com")};
            var link1 = new Vertex {Uri = new Uri("https://foobar.com/link1")};
            var link2 = new Vertex {Uri = new Uri("https://foobar.com/link2")};
            var link3 = new Vertex {Uri = new Uri("https://foobar.com/link3")};

            var seedGraphRepository = new SeedGraphRepository();

            seedGraphRepository.AddVertices(targetUri, link1, link2, link3);
            seedGraphRepository.AddEdges(
                new Edge {From = targetUri, To = link1},
                new Edge {From = link1, To = link2},
                new Edge {From = link2, To = link3}
            );

            // Act
            var targetUriResult = await seedGraphRepository.GetConnectedVerticesAsync(targetUri);
            var link1Result = await seedGraphRepository.GetConnectedVerticesAsync(link1);
            var link2Result = await seedGraphRepository.GetConnectedVerticesAsync(link2);
            var link3Result = await seedGraphRepository.GetConnectedVerticesAsync(link3);

            // Assert
            targetUriResult.Should().BeEquivalentTo(new List<Vertex>
            {
                targetUri,
                link1,
                link2,
                link3
            });
            link1Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link1,
                link2,
                link3
            });
            link2Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link2,
                link3
            });
            link3Result.Should().BeEquivalentTo(new List<Vertex>
            {
                link3
            });
        }
    }
}