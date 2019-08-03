using System.Collections.Generic;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public static class CrawlerJobResponseUtility
    {
        private static readonly Dictionary<CrawlerJobInfo.WorkerStatus, CrawlerJobResponse.JobStatus> StatusMapping = new Dictionary<CrawlerJobInfo.WorkerStatus, CrawlerJobResponse.JobStatus>()
        {
            {CrawlerJobInfo.WorkerStatus.Accepted, CrawlerJobResponse.JobStatus.InProgress},
            {CrawlerJobInfo.WorkerStatus.InProgress, CrawlerJobResponse.JobStatus.InProgress},
            {CrawlerJobInfo.WorkerStatus.Complete, CrawlerJobResponse.JobStatus.Complete}
        };

        public static CrawlerJobResponse NewFromExistingJobInfo(CrawlerJobInfo existingJobInfo)
        {
            var status = StatusMapping
                .TryGetValue(existingJobInfo.Status, out var mappedStatus) 
                ? mappedStatus 
                : CrawlerJobResponse.JobStatus.Unknown;

            return new CrawlerJobResponse
            {
                Id = existingJobInfo.Id,
                Status = status,
                Result = existingJobInfo.Result
            };
        }

    }
}