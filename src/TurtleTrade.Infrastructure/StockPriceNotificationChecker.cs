using System;
using System.Collections.Generic;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;

namespace TurtleTrade.Infrastructure
{
    public class StockPriceNotificationChecker : IStockPriceNotificationChecker
    {
        private readonly HashSet<string> _checkers;
        private readonly string _dateFormat = "yyyy-MM-dd";

        public StockPriceNotificationChecker()
        {
            _checkers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool CanNotify(string memberEmail, string stockFullID, StockNotificationType type, DateTime today, BuySellStrategyType strategy, StockBuyState targetState)
        {
            return !_checkers.Contains(ComposeKey(memberEmail, stockFullID, today, type, strategy, targetState));
        }

        public void ClearOldData(DateTime dayToClear)
        {
            _checkers.RemoveWhere(s => s.Contains(dayToClear.ToString(_dateFormat)));
        }

        public void InsertRecord(string memberEmail, string stockFullID, StockNotificationType type, DateTime date, BuySellStrategyType strategy, StockBuyState targetState)
        {
            _checkers.Add(ComposeKey(memberEmail, stockFullID, date, type, strategy, targetState));
        }

        private string ComposeKey(string memberEmail, string stockFullID, DateTime date, StockNotificationType type, BuySellStrategyType strategy, StockBuyState targetState)
        {
            return $"{strategy.GetString()}.{memberEmail}.{stockFullID}.{date.ToString(_dateFormat)}.{type.ToString()}.{targetState.GetStockBuyStateValue()}";
        }
    }
}
