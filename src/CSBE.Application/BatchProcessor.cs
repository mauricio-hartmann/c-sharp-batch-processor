using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CSBE.Application
{
    internal sealed class BatchProcessor<T>
    {
        private readonly BlockingCollection<T> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public bool CanHandleMoreItems { get => !_cancellationTokenSource.IsCancellationRequested && !_queue.IsAddingCompleted; }

        public BatchProcessor(int batchSize)
        {
            _queue = new BlockingCollection<T>(batchSize);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public BatchProcessor(int batchSize, CancellationTokenSource cancellationTokenSource)
        {
            _queue = new BlockingCollection<T>(batchSize);
            _cancellationTokenSource = cancellationTokenSource;
        }

        public void Start(Action<T> action) => Task.Run(() => ProcessItems(action, _cancellationTokenSource.Token));

        public void BlockNewItems() => _queue.CompleteAdding();

        public void AbortProcessing() => _cancellationTokenSource.Cancel();

        public void Enqueue(T item)
        {
            if (CanHandleMoreItems)
            {
                try
                {
                    _queue.Add(item);
                }
                catch (InvalidOperationException)
                {
                    // Do nothing
                }
            }

        }

        private void ProcessItems(Action<T> action, CancellationToken cancellationToken)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var batch = _queue.GetConsumingEnumerable(cancellationToken);

                try
                {
                    foreach (var item in batch)
                    {
                        var task = Task.Run(() => action(item), cancellationToken);
                        task.Wait();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Stopping due to a cancel request");
                    BlockNewItems();
                }
                catch (Exception)
                {
                    Console.WriteLine("Stopping due to an unhandled exception");
                    BlockNewItems();
                    AbortProcessing();
                }
            }
        }
    }
}
