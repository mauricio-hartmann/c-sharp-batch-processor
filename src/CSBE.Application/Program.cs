using CSBE.Application;
using System;
using System.Threading.Tasks;

public class Program
{
    public static void Main(string[] args)
    {
        var processor = new BatchProcessor(30);
        processor.Start();

        for (int i = 0; i <= 50; i++)
        {
            var task = Task.Run(async () => await PrintAsync(i + 1));
            processor.EnqueuTask(task);
        }

        processor.Stop();
    }

    public static async Task PrintAsync(int value)
    {
        await Task.Delay(2);
        Console.WriteLine($"Task: {value}");
    }
}
