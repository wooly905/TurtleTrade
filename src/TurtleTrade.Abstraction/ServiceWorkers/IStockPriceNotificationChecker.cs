using System;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Abstraction.ServiceWorkers
{
    public enum StockNotificationType
    {
        Buy = 0,
        SellStop = 1,
    }

    public interface IStockPriceNotificationChecker
    {
        bool CanNotify(string memberEmail, string stockFullID, StockNotificationType type, DateTime today, BuySellStrategyType strategy, StockBuyState targetState);
        void InsertRecord(string memberEmail, string stockFullID, StockNotificationType type, DateTime date, BuySellStrategyType strategy, StockBuyState targetState);
        void ClearOldData(DateTime dayToClear);
    }
}
