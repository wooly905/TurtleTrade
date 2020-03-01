using System;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction;

namespace TurtleTrade.Infrastructure.Config
{
    public class TradingTime : ITradingTime
    {
        public TradingTime(string country, DateTime tradingStartTime, DateTime tradingEndTime, DateTime dailyPriceImportTime, DateTime historicalPriceImportTime)
        {
            Country = country.GetCountryKindFromShortName();
            TradingStartTime = tradingStartTime;
            TradingEndTime = tradingEndTime;
            DailyPriceImportTime = dailyPriceImportTime;
            HistoricalPriceImportTime = historicalPriceImportTime;
        }

        public CountryKind Country { get; set; }

        public DateTime DailyPriceImportTime { get; set; }

        public DateTime HistoricalPriceImportTime { get; set; }

        public DateTime TradingEndTime { get; set; }

        public DateTime TradingStartTime { get; set; }
    }
}
