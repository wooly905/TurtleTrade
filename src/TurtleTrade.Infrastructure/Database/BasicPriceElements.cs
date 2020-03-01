using System;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    internal class AllPricesEntry : IAllPricesEntry
    {
        public AllPricesEntry(CountryKind country,
                              string stockID,
                              decimal lowPrice,
                              decimal highPrice,
                              decimal closePrice,
                              decimal openPrice,
                              DateTime tradeDateTime,
                              string yearRange,
                              int volume,
                              decimal? atr,
                              decimal? n20,
                              decimal? highIn20,
                              decimal? lowIn10,
                              decimal? n40,
                              decimal? highIn40,
                              decimal? lowIn15,
                              decimal? n60,
                              decimal? highIn60,
                              decimal? lowIn20,
                              decimal? ma20,
                              decimal? ma40,
                              decimal? ma60,
                              decimal? ma120,
                              decimal? ma240)
        {
            Country = country;
            StockId = stockID;
            LowPrice = lowPrice;
            HighPrice = highPrice;
            ClosePrice = closePrice;
            OpenPrice = openPrice;
            TradeDateTime = tradeDateTime;
            YearRange = yearRange;
            Volume = volume;
            ATR = atr;
            N20 = n20;
            HighIn20 = highIn20;
            LowIn10 = lowIn10;
            N40 = n40;
            HighIn40 = highIn40;
            LowIn15 = lowIn15;
            N60 = n60;
            HighIn60 = highIn60;
            LowIn20 = lowIn20;
            MA20 = ma20;
            MA40 = ma40;
            MA60 = ma60;
            MA120 = ma120;
            MA240 = ma240;
        }

        public decimal? ATR { get; private set; }

        public decimal ClosePrice { get; private set; }

        public CountryKind Country { get; private set; }

        public decimal? HighIn20 { get; private set; }

        public decimal? HighIn40 { get; private set; }

        public decimal? HighIn60 { get; private set; }

        public decimal HighPrice { get; private set; }

        public decimal? LowIn10 { get; private set; }

        public decimal? LowIn15 { get; private set; }

        public decimal? LowIn20 { get; private set; }

        public decimal LowPrice { get; private set; }

        public decimal? MA120 { get; private set; }

        public decimal? MA20 { get; private set; }

        public decimal? MA240 { get; private set; }

        public decimal? MA40 { get; private set; }

        public decimal? MA60 { get; private set; }

        public decimal? N20 { get; private set; }

        public decimal? N40 { get; private set; }

        public decimal? N60 { get; private set; }

        public decimal OpenPrice { get; private set; }

        public string StockId { get; private set; }

        public DateTime TradeDateTime { get; private set; }

        public int Volume { get; private set; }

        public string YearRange { get; private set; }
    }
}
