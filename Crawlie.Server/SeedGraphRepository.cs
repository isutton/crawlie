using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class Vertex
    {
        public Uri Uri { get; set; }
    }

    public class Edge
    {
        public Vertex From { get; set; }
        
        public Vertex To { get; set; }
    }
    
    public class SeedGraphRepository
    {
        public SeedGraphRepository()
        {
            Vertices = new HashSet<Vertex>();
            Edges = new HashSet<Edge>();
        }

        private HashSet<Vertex> Vertices { get; set; }
        
        private HashSet<Edge> Edges { get; set; }

        public void AddEdges(params Edge[] edges)
        {
            Edges.UnionWith(edges);
        }

        public void AddVertices(params Vertex[] vertices)
        {
            Vertices.UnionWith(vertices);
        }

        private async Task GetConnectedVerticesInternalAsync(Uri targetUri, ISet<Vertex> verticesSet)
        {
            var vertex = Vertices.FirstOrDefault(v => v.Uri == targetUri);
            if (vertex == null) return;
            
            await GetConnectedVerticesInternalAsync(vertex, verticesSet);
        }
        
        private async Task GetConnectedVerticesInternalAsync(Vertex vertex, ISet<Vertex> verticesSet)
        {
            var connectedVertices = GetConnectedVertices(vertex);

            connectedVertices.ForEach(v => verticesSet.Add(v));
            verticesSet.Add(vertex);

            var tasks = connectedVertices.Select(v => GetConnectedVerticesInternalAsync(v, verticesSet));
            await Task.WhenAll(tasks);
        }

        public async Task<List<Vertex>> GetConnectedVerticesAsync(Uri targetUri)
        {
            var connectedVertices = new HashSet<Vertex>();
            await GetConnectedVerticesInternalAsync(targetUri, connectedVertices);
            return connectedVertices.ToList();
        }

        public async Task<List<Vertex>> GetConnectedVerticesAsync(Vertex vertex)
        {
            var connectedVertices = new HashSet<Vertex>();
            await GetConnectedVerticesInternalAsync(vertex, connectedVertices);
            return connectedVertices.ToList();
        }
        
        private List<Vertex> GetConnectedVertices(Vertex vertex)
        {
            return Edges
                .Where(e => e.From.Uri == vertex.Uri)
                .Select(e => e.To)
                .ToList();
        }

        public bool Contains(Uri seedUri)
        {
            return Vertices.FirstOrDefault(v => v.Uri == seedUri) != null;
        }
    }
}