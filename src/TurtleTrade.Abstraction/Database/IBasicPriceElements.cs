using System;

namespace TurtleTrade.Abstraction.Database
{
    public interface IBasicPriceElements
    {
        decimal ClosePrice { get; }  // a.k.a. Latest Price

        decimal HighPrice { get; }

        decimal LowPrice { get; }

        decimal OpenPrice { get; }

        DateTime TradeDateTime { get; }

        int Volume { get; }
    }
}
