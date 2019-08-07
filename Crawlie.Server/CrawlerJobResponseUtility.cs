using System.Collections.Generic;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public static class CrawlerJobResponseUtility
    {
        private static readonly Dictionary<SeedJobStatus.WorkerStatus, SeedJobStatusResponse.JobStatus> StatusMapping = new Dictionary<SeedJobStatus.WorkerStatus, SeedJobStatusResponse.JobStatus>()
        {
            {SeedJobStatus.WorkerStatus.Accepted, SeedJobStatusResponse.JobStatus.InProgress},
            {SeedJobStatus.WorkerStatus.InProgress, SeedJobStatusResponse.JobStatus.InProgress},
            {SeedJobStatus.WorkerStatus.Complete, SeedJobStatusResponse.JobStatus.Complete}
        };

        public static SeedJobStatusResponse NewFromExistingJobInfo(SeedJobStatus existingJobStatus)
        {
            var status = StatusMapping
                .TryGetValue(existingJobStatus.Status, out var mappedStatus) 
                ? mappedStatus 
                : SeedJobStatusResponse.JobStatus.Unknown;

            return new SeedJobStatusResponse
            {
                Id = existingJobStatus.Id,
                Status = status,
                Result = existingJobStatus.Result
            };
        }

    }
}