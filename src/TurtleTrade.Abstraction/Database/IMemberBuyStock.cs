using System;

namespace TurtleTrade.Abstraction.Database
{
    public interface IMemberBuyStock : IMember, IStockID
    {
        DateTime BuyDate { get; }

        decimal BuyPrice { get; }

        decimal NValue { get; }

        StockBuyState State { get; }

        decimal StopPrice { get; }

        BuySellStrategyType Strategy { get; }
    }
}
