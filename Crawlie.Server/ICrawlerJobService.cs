using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ICrawlerJobService
    {
        Task<SeedJobStatusResponse> HandleJobRequest(SeedJobRequest jobRequest);
        
        Task<SeedJobStatusResponse> GetJobInfo(string jobId);
    }
}