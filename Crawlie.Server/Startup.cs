using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Crawlie.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpClient();

            // This component handles the link crawling operation.
            services.AddSingleton<ICrawlerEngine, CrawlerEngine>();
            
            // This service is used to download documents through HttpClient.
            services.AddSingleton<IDocumentFetcher, HttpClientDocumentFetcher>();
            
            // The CrawlerJobController interacts with ICrawlerJobService to
            // post new jobs and get information about submitted jobs.
            services.AddSingleton<ICrawlerJobService, DefaultCrawlerJobService>();
            
            // The ICrawlerRepository is used by the DefaultCrawlerJobService
            // to query and add jobs and CrawlerWorker processes to complete
            // their jobs.
            services.AddSingleton<ISeedJobRepository, ConcurrentSeedJobRepository>();
            
            // The CrawlerJobController pumps requests in the ICrawlerWorkerQueue,
            // which in turn are consumed by CrawlerWorker processes.
            services.AddSingleton<ICrawlerWorkerQueue, DefaultCrawlerWorkerQueue>();

            // This is the glue between ASP.NET host and the CrawlerWorker
            // processes.
            services.AddHostedService<CrawlerBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}