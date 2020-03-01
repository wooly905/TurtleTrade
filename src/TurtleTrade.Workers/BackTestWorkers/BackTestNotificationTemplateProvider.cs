using System;
using System.Collections.Generic;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.ServiceWorkers.BackTestWorkers
{
    internal class BackTestNotificationTemplateProvider : IEmailTemplateProvider
    {
        private DateTime _currentDate;

        public void SetDateTime(DateTime currentDate)
        {
            _currentDate = currentDate;
        }

        public IEmailTemplate GetAddOnBuyEmailTemplate(string receipent, string stockId, string stockName, decimal newStopPrice, StockBuyState newState)
        {
            return new BackTestNotificationContent(stockId,
                                                   "Buy",
                                                   "AddOn",
                                                   newStopPrice,
                                                   _currentDate,
                                                   $"NewStopPrice={newStopPrice} and New StockBuyState={newState}");
        }

        public IEmailTemplate GetBreakDownEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal lowPriceInStrategy, decimal currentLowPrice)
        {
            return new BackTestNotificationContent(stockId,
                                                   "Sell",
                                                   "BreakDown",
                                                   currentLowPrice,
                                                   _currentDate,
                                                   $"Low Price in SystemN :{lowPriceInStrategy}");
        }

        public IEmailTemplate GetBreakThroughEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal highPriceInStrategy, decimal currentHighPrice)
        {
            return new BackTestNotificationContent(stockId,
                                                   "Buy",
                                                   "FirstBuy",
                                                   currentHighPrice,
                                                   _currentDate,
                                                   "");
        }

        public IEmailTemplate GetDailyPriceImportFailEmailTemplate()
        {
            return null;
        }

        public IEmailTemplate GetStopLossEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal stopPriceInStrategy, decimal currentLowPrice)
        {
            return new BackTestNotificationContent(stockId,
                                                   "Sell",
                                                   "StopLoss",
                                                   currentLowPrice,
                                                   _currentDate,
                                                   $"Stop Price in SystemN :{stopPriceInStrategy}");
        }

        public IEmailTemplate GetHistoricalDataImportEmailTemplate(string receipent, IReadOnlyList<IHistoricalDataWaitingEntry> entries)
        {
            // TODO : add it later
            return null;
        }
    }
}
