using CSBE.Application;
using System;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public static void Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        Action<int> action = new Action<int>((item) =>
        {
            Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            Console.WriteLine($"Printing value: {item}");

            if (item > 3)
                //cancellationTokenSource.Cancel();
                throw new Exception();
        });

        var processor = new BatchProcessor<int>(1, cancellationTokenSource);

        try
        {
            processor.Start(action);
        }
        catch (Exception)
        {
            processor.AbortProcessing();
        }

        for (int i = 0; i <= 10; i++)
        {
            if (processor.CanHandleMoreItems)
            {
                var value = i + 1;
                Console.WriteLine($"Adding value: {value}");
                processor.Enqueue(value);
            }
            else
            {
                break;
            }
        }

        processor.BlockNewItems();

        if (cancellationTokenSource.IsCancellationRequested is false)
        {
            // Wait avoiding main thread shut down while other threads are runnning
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}
