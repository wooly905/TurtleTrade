using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Storage;

namespace TurtleTrade.Abstraction
{
    public interface IDataAccessToolProvider
    {
        IDatabaseOperations DatabaseOperations { get; }

        ICurrentPriceStorage MemoryStorage { get; }
    }
}
