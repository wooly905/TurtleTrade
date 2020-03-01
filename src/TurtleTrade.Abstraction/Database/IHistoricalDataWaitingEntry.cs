using System;
using System.Threading.Tasks;

namespace TurtleTrade.Abstraction.Database
{
    public interface IHistoricalDataWaitingEntry : IStockID
    {
        DateTime DataEndDate { get; }

        DateTime DataStartDate { get; }

        HistoricalDataWaitingState State { get; }
    }
}
