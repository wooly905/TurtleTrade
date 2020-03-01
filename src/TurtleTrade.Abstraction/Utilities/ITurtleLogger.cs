using System;
using System.Threading.Tasks;

namespace TurtleTrade.Abstraction.Utilities
{
    public interface ITurtleLogger
    {
        void Dispose();

        Task WriteToCurrentPriceLogAsync(CountryKind country, string data);

        Task WriteToEmailLogAsync(CountryKind country, DateTime time, string workerKind, string emailContent);

        Task WriteToErrorLogAsync(CountryKind country, DateTime time, string workerKind, Exception ex);

        Task WriteToHeartBeatLogAsync(CountryKind country, DateTime time, string workerKind);

        Task WriteToWorkerLogAsync(CountryKind country, DateTime time, string workerKind, string message);
    }
}
