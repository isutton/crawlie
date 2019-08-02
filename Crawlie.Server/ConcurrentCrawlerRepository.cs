using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class ConcurrentCrawlerRepository : ICrawlerRepository
    {
        private readonly ConcurrentDictionary<string, CrawlerJobInfo> _jobCollection = new ConcurrentDictionary<string, CrawlerJobInfo>();
        
        public ConcurrentCrawlerRepository(params CrawlerJobInfo[] existingJobs)
        {
            foreach (var jobInfo in existingJobs)
            {
                _jobCollection.TryAdd(jobInfo.Id, jobInfo);
            }
        }
        
        public Task<CrawlerJobInfo> GetJobInfoAsync(CrawlerJobRequest jobRequest)
        {
            var jobId = jobRequest.Uri.ToString();
            return Task.FromResult(_jobCollection.TryGetValue(jobId, out var jobInfo) ? jobInfo : null);
        }

        public Task<CrawlerJobInfo> AddJobRequestAsync(CrawlerJobRequest jobRequest)
        {
            var jobInfo = new CrawlerJobInfo()
            {
                Id = jobRequest.Uri.ToString(),
                Status = CrawlerJobInfo.WorkerStatus.Accepted
            };
            
            return Task.FromResult(_jobCollection.TryAdd(jobRequest.Uri.ToString(), jobInfo) ? jobInfo : null);
        }

        public Task CompleteJobAsync(string jobId, List<Uri> documentLinks)
        {
            if (_jobCollection.TryGetValue(jobId, out var jobInfo))
            {
                var newJobInfo = new CrawlerJobInfo()
                {
                    Id = jobInfo.Id,
                    Status = jobInfo.Status,
                    Result = documentLinks
                };

                if (!_jobCollection.TryUpdate(jobId, newJobInfo, jobInfo))
                {
                    throw new NotImplementedException();
                }
            }

            return Task.CompletedTask;
        }
    }
}