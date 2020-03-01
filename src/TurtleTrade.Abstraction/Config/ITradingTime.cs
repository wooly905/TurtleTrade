using System;
using TurtleTrade.Abstraction;

namespace TurtleTrade.Abstraction.Config
{
    public interface ITradingTime
    {
        CountryKind Country { get; set; }

        DateTime DailyPriceImportTime { get; set; }

        DateTime HistoricalPriceImportTime { get; set; }

        DateTime TradingEndTime { get; set; }

        DateTime TradingStartTime { get; set; }
    }
}
