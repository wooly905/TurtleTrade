using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    internal class Stock : IStock
    {
        public Stock(CountryKind country, string stockID, string stockName, string stockExchangeID)
        {
            Country = country;
            StockId = stockID;
            StockName = stockName;
            StockExchangeID = stockExchangeID;
        }

        public string StockName { get; }

        public string StockExchangeName { get; set; }

        public string Description { get; set; }

        public string StockExchangeID { get; }

        public CountryKind Country { get; }

        public string StockId { get; }
    }
}
