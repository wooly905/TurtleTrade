using System;

namespace TurtleTrade.Abstraction.Database
{
    public interface IStockPriceHistory : IStockID, ITurtlePriceEntry, IBasicPriceElements
    {                  
        string PriceChange { get; }

        string PriceRange { get; }

        string YearRange { get; }
    }
}
