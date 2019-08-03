using System;
using System.Collections.Generic;
using System.Net.Http;
using CommandLine;

namespace Crawlie.Client
{
    class Program
    {
        private class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('c', "hostname", Required = false, HelpText = "Set the Crawlie server to submit requests.")]
            public string ServerHostname { get; set; } = "https://localhost:5001";
            
            [Value(0, Required = true, HelpText = "The URL to process.")]
            public string Url { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(async o =>
                {
                    var httpClient = new HttpClient();
                    var crawlerClientConfiguration = new CrawlerClientConfiguration()
                    {
                        BaseAddress = new Uri(o.ServerHostname)
                    };
                    var crawlerClient = new CrawlerClient(httpClient, crawlerClientConfiguration);

                    var jobResponse = await crawlerClient.GetJobRequestAsync(new Uri(o.Url));
                    
                    Console.WriteLine("Hello World!");
                });
        }
    }
}