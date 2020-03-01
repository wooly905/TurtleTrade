using System;
using System.Runtime.CompilerServices;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

[assembly: InternalsVisibleTo("Turtle2.ServiceWorkers")]
[assembly: InternalsVisibleTo("Turtle2.Impl")]

namespace TurtleTrade.Database
{
    internal class StockPriceHistory : IStockPriceHistory
    {
        public StockPriceHistory(CountryKind country, string stockID, DateTime tradeDate)
        {
            Country = country;
            StockId = stockID;
            TradeDateTime = tradeDate;
        }

        public decimal? ATR { get; set; }

        public decimal ClosePrice { get; set; }

        public CountryKind Country { get; private set; }

        public decimal? HighIn20 { get; set; }

        public decimal? HighIn40 { get; set; }

        public decimal? HighIn60 { get; set; }

        public decimal HighPrice { get; set; }

        public decimal? LowIn10 { get; set; }

        public decimal? LowIn15 { get; set; }

        public decimal? LowIn20 { get; set; }

        public decimal LowPrice { get; set; }

        public decimal? MA120 { get; set; }

        public decimal? MA20 { get; set; }

        public decimal? MA240 { get; set; }

        public decimal? MA40 { get; set; }

        public decimal? MA60 { get; set; }

        public decimal? N20 { get; set; }

        public decimal? N40 { get; set; }

        public decimal? N60 { get; set; }

        public decimal OpenPrice { get; set; }

        public string PriceChange { get; set; }

        public string PriceRange { get; set; }

        public string StockId { get; private set; }

        public DateTime TradeDateTime { get; private set; }

        public int Volume { get; set; }

        public string YearRange { get; set; }
    }
}
