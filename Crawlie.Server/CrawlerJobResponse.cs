using System;
using System.Collections.Generic;

namespace Crawlie.Server
{
    public class CrawlerJobResponse
    {
        private static readonly Dictionary<CrawlerJobInfo.WorkerStatus, JobStatus> StatusMapping = new Dictionary<CrawlerJobInfo.WorkerStatus, JobStatus>()
        {
            {CrawlerJobInfo.WorkerStatus.Accepted, JobStatus.InProgress},
            {CrawlerJobInfo.WorkerStatus.InProgress, JobStatus.InProgress},
            {CrawlerJobInfo.WorkerStatus.Complete, JobStatus.Complete}
        };
        
        public CrawlerJobResponse(CrawlerJobInfo existingJobInfo)
        {
            Id = existingJobInfo.Id;
            Status = StatusMapping[existingJobInfo.Status];
            Result = existingJobInfo.Result;
        }

        public enum JobStatus
        {
            Accepted = 1,
            InProgress = 2,
            Complete = 3
        }
        
        public string Id { get; set; }
        
        public JobStatus Status { get; set; } 
        
        public List<Uri> Result { get; set; }
    }
}