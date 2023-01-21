using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CSBE.Application
{
    internal sealed class BatchProcessor
    {
        private readonly BlockingCollection<Task> _taskQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BatchProcessor(int batchSize)
        {
            _taskQueue = new BlockingCollection<Task>(batchSize);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start() => Task.Run(() => ProcessTasksAsync(_cancellationTokenSource.Token));

        public void Stop() => _cancellationTokenSource.Cancel();

        public void EnqueuTask(Task task) => _taskQueue.Add(task);

        private async Task ProcessTasksAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var batch = _taskQueue.GetConsumingEnumerable(cancellationToken);
                await Task.WhenAll(batch);
            }
        }
    }
}
