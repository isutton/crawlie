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

        public Task<SeedJobStatus> GetJobInfoAsync(Uri targetUri)
        {
            throw new NotImplementedException();
        }

        public Task<SeedJobStatus> AddJobRequestAsync(SeedJobRequest jobRequest)
        {
            throw new NotImplementedException();
        }

        public void CompleteJob(Uri targetUri, List<Uri> documentLinks)
        {
            throw new NotImplementedException();
        }
    }
}