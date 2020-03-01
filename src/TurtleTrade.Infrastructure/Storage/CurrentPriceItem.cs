using System;
using TurtleTrade.Abstraction.Storage;

namespace TurtleTrade.Infrastructure.Storage
{
    internal class CurrentPriceItem : ICurrentPrice
    {
        public CurrentPriceItem(decimal currentPrice, DateTime lastTradeTime, decimal todayHigh, decimal todayLow)
        {
            CurrentPrice = currentPrice;
            LastTradeTime = lastTradeTime;
            TodayHighPrice = todayHigh;
            TodayLowPrice = todayLow;
        }

        public decimal CurrentPrice { get; }
        public DateTime LastTradeTime { get; }
        public decimal TodayHighPrice { get; }
        public decimal TodayLowPrice { get; }

        public override bool Equals(object obj)
        {
            return !(obj is CurrentPriceItem currentPriceItem)
                ? false
                : CurrentPrice == currentPriceItem.CurrentPrice
                  && LastTradeTime == currentPriceItem.LastTradeTime
                  && TodayHighPrice == currentPriceItem.TodayHighPrice
                  && TodayLowPrice == currentPriceItem.TodayLowPrice;
        }

        public static bool operator ==(CurrentPriceItem item1, CurrentPriceItem item2)
        {
            return item1 is null ? item2 is null : item1.Equals(item2);
        }

        public static bool operator !=(CurrentPriceItem item1, CurrentPriceItem item2)
        {
            return !item1.Equals(item2);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
