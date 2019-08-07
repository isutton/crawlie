using System;
using System.Threading.Tasks;
using Crawlie.Contracts;

namespace Crawlie.Server
{
    public class DefaultSeedJobService : ISeedJobService
    {
        private readonly ISeedJobRepository _repository;
        private readonly ISeedWorkerQueue _workerQueue;

        public DefaultSeedJobService(
            ISeedJobRepository repository,
            ISeedWorkerQueue workerQueue)
        {
            _repository = repository;
            _workerQueue = workerQueue;
        }

        public async Task<SeedJobStatusResponse> HandleJobRequest(SeedJobRequest jobRequest)
        {
            var existingJobInfo = await _repository.GetAsync(jobRequest.Uri);
            if (existingJobInfo != null)
                // Requested URL is in progress or finished, map jobInfo
                // to a response.
                return CrawlerJobResponseUtility.NewFromExistingJobInfo(existingJobInfo);

            // Requested URL is new, so add the job request to the
            // repository for the results to be collected later and
            // enqueue the jobRequest URL.
            var addedJobInfo = await _repository.AddAsync(jobRequest);
            _workerQueue.Add(jobRequest.Uri);

            return CrawlerJobResponseUtility.NewFromExistingJobInfo(addedJobInfo);
        }

        public async Task<SeedJobStatusResponse> GetJobInfo(string jobId)
        {
            var jobInfo = await _repository.GetAsync(new Uri(jobId));


            return
                jobInfo != null ? CrawlerJobResponseUtility.NewFromExistingJobInfo(jobInfo) : null;
        }
    }
}