using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class ConcurrentCrawlerRepository : ICrawlerRepository
    {
        private readonly ConcurrentDictionary<string, CrawlerJobInfo> _jobCollection = new ConcurrentDictionary<string, CrawlerJobInfo>();

        public static ConcurrentCrawlerRepository NewWithExistingJobs(params CrawlerJobInfo[] existingJobs)
        {
            var repository = new ConcurrentCrawlerRepository();
            repository.TryAddRange(existingJobs);
            return repository;
        }

        private void TryAddRange(CrawlerJobInfo[] existingJobs)
        {
            foreach (var jobInfo in existingJobs)
            {
                _jobCollection.TryAdd(jobInfo.Id, jobInfo);
            }
        }
       
        public Task<CrawlerJobInfo> GetJobInfoAsync(CrawlerJobRequest jobRequest)
        {
            var jobId = jobRequest.Uri.ToString().TrimEnd('/');
            if (_jobCollection.TryGetValue(jobId, out var jobInfo))
            {
                return Task.FromResult(jobInfo);
            }

            return Task.FromResult<CrawlerJobInfo>(null);
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

        public void CompleteJob(string jobId, List<Uri> documentLinks)
        {
            if (_jobCollection.TryGetValue(jobId, out var jobInfo))
            {
                var newJobInfo = new CrawlerJobInfo()
                {
                    Id = jobInfo.Id,
                    Status = CrawlerJobInfo.WorkerStatus.Complete,
                    Result = documentLinks
                };

                if (!_jobCollection.TryUpdate(jobId, newJobInfo, jobInfo))
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}