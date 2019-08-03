using System.Threading.Tasks;
using System.Web;
using Crawlie.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Crawlie.Server.Jobs
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlerJobController : ControllerBase
    {
        private readonly ICrawlerJobService _crawlerJobService;

        public CrawlerJobController(
            ICrawlerJobService crawlerJobService)
        {
            _crawlerJobService = crawlerJobService;
        }
        
        [HttpPost]
        public async Task<ActionResult<CrawlerJobResponse>> CreateJob([FromBody] CrawlerJobRequest jobRequest)
        {
            var jobResponse = await _crawlerJobService.HandleJobRequest(jobRequest);
            return Ok(jobResponse);
        }

        [HttpGet]
        public async Task<ActionResult<CrawlerJobResponse>> GetJobInfo([FromQuery] string jobId)
        {
            var jobResponse = await _crawlerJobService.GetJobInfo(jobId);
            return jobResponse == null 
                ? (ActionResult) NotFound() 
                : Ok(jobResponse);
        }
    }
}