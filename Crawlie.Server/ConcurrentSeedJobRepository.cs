using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crawlie.Contracts;
using Microsoft.Extensions.Logging;

namespace Crawlie.Server
{
    /// <summary>
    ///     ConcurrentCrawlerRepository is a repository that store current and
    ///     past crawler job results.
    /// </summary>
    public class ConcurrentSeedJobRepository : ISeedJobRepository
    {
        private readonly ConcurrentDictionary<string, SeedJobStatus> _jobCollection =
            new ConcurrentDictionary<string, SeedJobStatus>();

        public Task<SeedJobStatus> GetSeedJobStatusAsync(Uri seedUri)
        {
            return _jobCollection.TryGetValue(seedUri.ToString(), out var jobInfo)
                ? Task.FromResult(jobInfo)
                : Task.FromResult<SeedJobStatus>(null);
        }

        public Task<SeedJobStatus> AddJobRequestAsync(SeedJobRequest jobRequest)
        {
            var jobInfo = new SeedJobStatus
            {
                Id = jobRequest.Uri.ToString(),
                Status = SeedJobStatus.WorkerStatus.Accepted
            };

            return _jobCollection.TryAdd(jobRequest.Uri.ToString(), jobInfo)
                ? Task.FromResult(jobInfo)
                : Task.FromResult<SeedJobStatus>(null);
        }

        public void CompleteJob(Uri targetUri, List<Uri> documentLinks)
        {
            if (!_jobCollection.TryGetValue(targetUri.ToString(), out var jobInfo)) return;

            var newJobInfo = new SeedJobStatus
            {
                Id = jobInfo.Id,
                Status = SeedJobStatus.WorkerStatus.Complete,
                Result = documentLinks
            };

            if (!_jobCollection.TryUpdate(targetUri.ToString(), newJobInfo, jobInfo))
                throw new NotImplementedException("Work in progress.");
        }

        public void TryAddRange(IEnumerable<SeedJobStatus> existingJobs)
        {
            foreach (var jobInfo in existingJobs) _jobCollection.TryAdd(jobInfo.Id, jobInfo);
        }
    }
}