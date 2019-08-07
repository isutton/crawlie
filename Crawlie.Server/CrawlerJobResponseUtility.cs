using System.Collections.Generic;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public static class CrawlerJobResponseUtility
    {
        private static readonly Dictionary<SeedJobStatus.WorkerStatus, CrawlerJobResponse.JobStatus> StatusMapping = new Dictionary<SeedJobStatus.WorkerStatus, CrawlerJobResponse.JobStatus>()
        {
            {SeedJobStatus.WorkerStatus.Accepted, CrawlerJobResponse.JobStatus.InProgress},
            {SeedJobStatus.WorkerStatus.InProgress, CrawlerJobResponse.JobStatus.InProgress},
            {SeedJobStatus.WorkerStatus.Complete, CrawlerJobResponse.JobStatus.Complete}
        };

        public static CrawlerJobResponse NewFromExistingJobInfo(SeedJobStatus existingJobStatus)
        {
            var status = StatusMapping
                .TryGetValue(existingJobStatus.Status, out var mappedStatus) 
                ? mappedStatus 
                : CrawlerJobResponse.JobStatus.Unknown;

            return new CrawlerJobResponse
            {
                Id = existingJobStatus.Id,
                Status = status,
                Result = existingJobStatus.Result
            };
        }

    }
}