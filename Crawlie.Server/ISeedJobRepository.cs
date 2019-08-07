using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ISeedJobRepository
    {
        Task<SeedJobStatus> GetSeedJobStatusAsync(Uri seedUri);
        
        Task<SeedJobStatus> AddJobRequestAsync(SeedJobRequest jobRequest);
        
        void CompleteJob(Uri targetUri, List<Uri> documentLinks);
    }
}