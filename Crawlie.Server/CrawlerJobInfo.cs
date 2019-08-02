using System;
using System.Collections.Generic;

namespace Crawlie.Server
{
    public class CrawlerJobInfo
    {
        public enum WorkerStatus
        {
            Accepted = 1,
            InProgress = 2,
            Complete = 3
        }
        
        public string Id { get; set; }
        
        public WorkerStatus Status { get; set; } 
        

        public List<Uri> Result { get; set; }
    }
}