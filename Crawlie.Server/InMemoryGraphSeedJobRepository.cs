using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public class InMemoryGraphSeedJobRepository : ISeedJobRepository
    {
        private readonly SeedGraphRepository _seedGraphRepository;

        public InMemoryGraphSeedJobRepository()
        {
            _seedGraphRepository = new SeedGraphRepository();
        }

        public Task<SeedJobStatus> GetAsync(Uri seedUri)
        {
            var result = _seedGraphRepository.Contains(seedUri)
                ? new SeedJobStatus
                {
                    Id = seedUri.ToString(),
                    Status = SeedJobStatus.WorkerStatus.InProgress
                }
                : null;

            return Task.FromResult(result);
        }

        public Task<SeedJobStatus> AddAsync(SeedJobRequest jobRequest)
        {
            var seedJobStatus = new SeedJobStatus
            {
                Id = jobRequest.Uri.ToString(),
                Status = SeedJobStatus.WorkerStatus.Accepted
            };

            return Task.FromResult(seedJobStatus);
        }

        public void Complete(Uri seedUri, List<Uri> documentLinks)
        {
//            _seedGraphRepository.AddVertices(seedUri);
//            var edges = documentLinks.Select(uri => (seedUri, uri));
//            _seedGraphRepository.AddEdges(edges.ToArray());
        }
    }
}