using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crawlie.Client
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(IConfiguration configuration)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOptions();
                    services.Configure<RunnerOptions>(configuration.GetSection("RunnerOptions"));
                    services.Configure<CrawlerClientOptions>(configuration.GetSection("CrawlerClientOptions"));
                    services.AddLogging();
                    services.AddTransient<CrawlerClient>();
                    services.AddTransient<Runner>();
                    services.AddHttpClient<CrawlerClient>()
                        .ConfigurePrimaryHttpMessageHandler(() =>
                            new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                            })
                        .ConfigureHttpClient(client => { client.BaseAddress = new Uri("https://localhost:5001"); });
                });
            return hostBuilder;
        }

        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddCommandLine(args).Build();

            var host =
                CreateHostBuilder(configuration)
                    .Build();

            var runner = host.Services.GetService<Runner>();
            await runner.Run();
        }
    }
}