using System.Threading.Tasks;

namespace Crawlie.Server
{
    public class CrawlerService
    {
        private readonly ICrawlerRepository _repository;
        private readonly ICrawlerWorkerQueue _workerQueue;

        public CrawlerService(
            ICrawlerRepository repository,
            ICrawlerWorkerQueue workerQueue)
        {
            _repository = repository;
            _workerQueue = workerQueue;
        }

        public async Task<CrawlerJobResponse> HandleJobRequest(CrawlerJobRequest jobRequest)
        {
            var existingJobInfo = await _repository.GetJobInfoAsync(jobRequest);
            if (existingJobInfo != null)
                // Requested URL is in progress or finished, map jobInfo
                // to a response.
                return new CrawlerJobResponse(existingJobInfo);

            // Requested URL is new, so add the job request to the
            // repository for the results to be collected later and
            // enqueue the jobRequest URL.
            var addedJobInfo = await _repository.AddJobRequestAsync(jobRequest);
            _workerQueue.Add(jobRequest.Uri.ToString());


            return new CrawlerJobResponse(addedJobInfo);
        }
    }
}