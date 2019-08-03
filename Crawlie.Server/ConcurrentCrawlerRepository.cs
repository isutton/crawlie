using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crawlie.Server
{
    /// <summary>
    ///     ConcurrentCrawlerRepository is a repository that store current and
    ///     past crawler job results.
    /// </summary>
    public class ConcurrentCrawlerRepository : ICrawlerRepository
    {
        private readonly ConcurrentDictionary<string, CrawlerJobInfo> _jobCollection =
            new ConcurrentDictionary<string, CrawlerJobInfo>();

        public Task<CrawlerJobInfo> GetJobInfoAsync(Uri targetUri)
        {
            return _jobCollection.TryGetValue(targetUri.ToString(), out var jobInfo)
                ? Task.FromResult(jobInfo)
                : Task.FromResult<CrawlerJobInfo>(null);
        }

        public Task<CrawlerJobInfo> AddJobRequestAsync(CrawlerJobRequest jobRequest)
        {
            var jobInfo = new CrawlerJobInfo
            {
                Id = jobRequest.Uri.ToString(),
                Status = CrawlerJobInfo.WorkerStatus.Accepted
            };

            return _jobCollection.TryAdd(jobRequest.Uri.ToString(), jobInfo)
                ? Task.FromResult(jobInfo)
                : Task.FromResult<CrawlerJobInfo>(null);
        }

        public void CompleteJob(Uri targetUri, List<Uri> documentLinks)
        {
            if (!_jobCollection.TryGetValue(targetUri.ToString(), out var jobInfo)) return;

            var newJobInfo = new CrawlerJobInfo
            {
                Id = jobInfo.Id,
                Status = CrawlerJobInfo.WorkerStatus.Complete,
                Result = documentLinks
            };

            if (!_jobCollection.TryUpdate(targetUri.ToString(), newJobInfo, jobInfo))
                throw new NotImplementedException("Work in progress.");
        }

        public void TryAddRange(IEnumerable<CrawlerJobInfo> existingJobs)
        {
            foreach (var jobInfo in existingJobs) _jobCollection.TryAdd(jobInfo.Id, jobInfo);
        }
    }
}