namespace Crawlie.Server
{
    public interface ICrawlerWorkerQueue
    {
        void Add(string uriString);

        string Take();
    }
}