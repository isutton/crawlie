using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ISeedJobRepository
    {
        Task<SeedJobStatus> GetAsync(Uri seedUri);
        
        Task<SeedJobStatus> AddAsync(SeedJobRequest jobRequest);
        
        void Complete(Uri seedUri, List<Uri> documentLinks);
    }
}