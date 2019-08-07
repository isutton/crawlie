using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public interface ISeedJobService
    {
        Task<SeedJobStatusResponse> HandleJobRequest(SeedJobRequest jobRequest);
        
        Task<SeedJobStatusResponse> GetJobInfo(string jobId);
    }
}