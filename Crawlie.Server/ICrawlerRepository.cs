using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ICrawlerRepository
    {
        Task<CrawlerJobInfo> GetJobInfoAsync(Uri targetUri);
        
        Task<CrawlerJobInfo> AddJobRequestAsync(CrawlerJobRequest jobRequest);
        
        void CompleteJob(Uri targetUri, List<Uri> documentLinks);
    }
}