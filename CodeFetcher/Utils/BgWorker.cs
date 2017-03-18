using System.ComponentModel;

namespace CodeFetcher
{
    public class BgWorker : BackgroundWorker
    {
        public BgWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }
    }
}
