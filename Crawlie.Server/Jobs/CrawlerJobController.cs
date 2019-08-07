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
        private readonly ISeedJobService _seedJobService;

        public CrawlerJobController(
            ISeedJobService seedJobService)
        {
            _seedJobService = seedJobService;
        }
        
        [HttpPost]
        public async Task<ActionResult<SeedJobStatusResponse>> CreateJob([FromBody] SeedJobRequest jobRequest)
        {
            var jobResponse = await _seedJobService.HandleJobRequest(jobRequest);
            return Ok(jobResponse);
        }

        [HttpGet]
        public async Task<ActionResult<SeedJobStatusResponse>> GetJobInfo([FromQuery] string jobId)
        {
            var jobResponse = await _seedJobService.GetJobInfo(jobId);
            return jobResponse == null 
                ? (ActionResult) NotFound() 
                : Ok(jobResponse);
        }
    }
}