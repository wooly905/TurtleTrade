using System;

namespace TurtleTrade.ServiceWorkers.BackTestWorkers
{
    internal class BackTestBuySellRecord
    {
        public string Action { get; set; }

        public string Comment { get; set; }

        public DateTime Date { get; set; }

        public string Op { get; set; }

        public decimal Price { get; set; }

        public string StockId { get; set; }
    }
}
