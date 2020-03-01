using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction.ServiceWorkers;

namespace TurtleTrade.Abstraction.ServiceWorkers
{
    public interface IServiceWorker
    {
        CountryKind Country { get; }

        ServiceWorkerKind Kind { get; }

        ServiceWorkerState State { get; }

        Task RunAsync(CancellationToken token);
    }
}
