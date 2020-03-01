using System.Threading;
using System.Threading.Tasks;

namespace TurtleTrade.Abstraction
{
    public interface ITradingStrategy
    {
        Task ExecuteAsync(CancellationToken token);
    }
}
