using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public interface ICrawlerRepository
    {
        Task<CrawlerJobInfo> GetJobInfoAsync(CrawlerJobRequest jobRequest);
        
        Task<CrawlerJobInfo> AddJobRequestAsync(CrawlerJobRequest jobRequest);
        
        void CompleteJob(string jobId, List<Uri> documentLinks);
    }
}