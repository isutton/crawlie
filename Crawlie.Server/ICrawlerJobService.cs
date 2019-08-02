using System.Threading.Tasks;

namespace Crawlie.Server
{
    public interface ICrawlerJobService
    {
        Task<CrawlerJobResponse> HandleJobRequest(CrawlerJobRequest jobRequest);
    }
}