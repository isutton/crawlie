using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public class GraphSeedJobRepository : ISeedJobRepository
    {

        private readonly SeedGraphRepository _seedGraphRepository;

        public GraphSeedJobRepository()
        {
            _seedGraphRepository = new SeedGraphRepository();
        }

        public Task<SeedJobStatus> GetAsync(Uri seedUri)
        {
            throw new NotImplementedException();
        }

        public Task<SeedJobStatus> AddAsync(SeedJobRequest jobRequest)
        {
            throw new NotImplementedException();
        }

        public void Complete(Uri seedUri, List<Uri> documentLinks)
        {
            throw new NotImplementedException();
        }
    }
}