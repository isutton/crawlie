using System;

namespace Crawlie.Client
{
    public class CrawlerClientOptions
    {
        public Uri BaseAddress { get; set; } = new Uri("https://localhost:5001");
    }
}