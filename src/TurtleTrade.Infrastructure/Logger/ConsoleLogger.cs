using System;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.Logger
{
    public class ConsoleLogger : ITurtleLogger
    {
        private readonly object _syncObject;

        public ConsoleLogger()
        {
            _syncObject = new object();
        }

        public Task WriteToWorkerLogAsync(CountryKind country, DateTime time, string workerKind, string message)
        {
            return PrintMessageOnConsoleAsync($"{workerKind} WorkerLog: {message}");
        }

        public Task WriteToHeartBeatLogAsync(CountryKind country, DateTime time, string workerKind)
        {
            return PrintMessageOnConsoleAsync($"{workerKind} sent a heartbeat.");
        }

        public Task WriteToErrorLogAsync(CountryKind country, DateTime time, string workerKind, Exception ex)
        {
            return PrintMessageOnConsoleAsync($"{workerKind} Error: {ex}", true);
        }

        public Task WriteToEmailLogAsync(CountryKind country, DateTime time, string workerKind, string emailContent)
        {
            return PrintMessageOnConsoleAsync($"{workerKind} Email: {emailContent}");
        }

        private Task PrintMessageOnConsoleAsync(string message, bool isError = false)
        {
            lock (_syncObject)
            {
                if (isError)
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Error : ");
                    Console.ForegroundColor = color;
                }

                Console.WriteLine(message);
            }

            return Task.CompletedTask;
        }

        public Task WriteToCurrentPriceLogAsync(CountryKind country, string data)
        {
            return PrintMessageOnConsoleAsync(data);
        }

        public void Dispose()
        {
        }
    }
}