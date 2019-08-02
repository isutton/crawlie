using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        public static CrawlerJobResponse NewFromExistingJobInfo(CrawlerJobInfo existingJobInfo)
        {
            var status = StatusMapping
                .TryGetValue(existingJobInfo.Status, out var mappedStatus) 
                ? mappedStatus 
                : JobStatus.Unknown;

            return new CrawlerJobResponse
            {
                Id = existingJobInfo.Id,
                Status = status,
                Result = existingJobInfo.Result
            };
        }
        
        public enum JobStatus
        {
            Unknown = 0,
            Accepted = 1,
            InProgress = 2,
            Complete = 3
        }
        
        public string Id { get; set; }
        
        public JobStatus Status { get; set; } 
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Uri> Result { get; set; }
    }
}