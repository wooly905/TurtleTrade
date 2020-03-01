using System;
using System.Collections.Generic;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        public IEmailTemplate GetAddOnBuyEmailTemplate(string receipent, string stockId, string stockName, decimal newStopPrice, StockBuyState newState)
        {
            return new AddOnBuyEmailTemplate(receipent, stockId, stockName, newStopPrice, newState);
        }

        public IEmailTemplate GetBreakDownEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal lowPriceInStrategy, decimal currentLowPrice)
        {
            return new BreakDownEmailTemplate(receipent, stockId, stockName, strategy, lowPriceInStrategy, currentLowPrice);
        }

        public IEmailTemplate GetBreakThroughEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal todayHighPrice, decimal high20Price)
        {
            return new BreakThroughEmailTemplate(receipent, stockId, stockName, strategy, todayHighPrice, high20Price);
        }

        public IEmailTemplate GetDailyPriceImportFailEmailTemplate()
        {
            throw new NotImplementedException();
        }

        public IEmailTemplate GetHistoricalDataImportEmailTemplate(string receipent, IReadOnlyList<IHistoricalDataWaitingEntry> entries)
        {
            return new HistoricalEmailTemplate(receipent, entries);
        }

        public IEmailTemplate GetStopLossEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal stopPriceInStrategy, decimal currentLowPrice)
        {
            return new StopLossEmailTemplate(receipent, stockId, stockName, strategy, stopPriceInStrategy, currentLowPrice);
        }
    }
}
