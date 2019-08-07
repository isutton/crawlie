using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ICrawlerJobService
    {
        Task<CrawlerJobResponse> HandleJobRequest(SeedJobRequest jobRequest);
        
        Task<CrawlerJobResponse> GetJobInfo(string jobId);
    }
}