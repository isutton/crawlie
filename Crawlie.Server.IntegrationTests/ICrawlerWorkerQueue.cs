namespace Crawlie.Server.IntegrationTests
{
    public interface ICrawlerWorkerQueue
    {
        void Add(string uriString);

        string Take();
    }
}