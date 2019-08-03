using CommandLine;

namespace Crawlie.Client
{
    internal class RunnerOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('c', "hostname", Required = false, HelpText = "Set the Crawlie server to submit requests.")]
        public string ServerHostname { get; } = "https://localhost:5001";

        [Value(0, Required = true, HelpText = "The URL to process.")]
        public string Url { get; set; }
    }
}