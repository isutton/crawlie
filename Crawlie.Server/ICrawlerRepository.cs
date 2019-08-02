using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public interface ICrawlerRepository
    {
        Task<CrawlerJobInfo> GetJobInfoAsync(CrawlerJobRequest jobRequest);
        
        Task<CrawlerJobInfo> AddJobRequestAsync(CrawlerJobRequest jobRequest);
        
        Task CompleteJobAsync(string jobId, List<Uri> documentLinks);
    }
}