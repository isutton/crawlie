using System.Threading.Tasks;
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
    }
}