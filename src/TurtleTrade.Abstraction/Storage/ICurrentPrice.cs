using System;

namespace TurtleTrade.Abstraction.Storage
{
    public interface ICurrentPrice
    {
        decimal CurrentPrice { get; }

        DateTime LastTradeTime { get; }

        decimal TodayHighPrice { get; }

        decimal TodayLowPrice { get; }
    }
}
