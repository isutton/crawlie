using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crawlie.Contracts
{
    public class SeedJobStatusResponse
    {
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