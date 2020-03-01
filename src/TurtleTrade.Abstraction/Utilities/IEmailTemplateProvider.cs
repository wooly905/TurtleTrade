using System.Collections.Generic;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Abstraction.Utilities
{
    public interface IEmailTemplateProvider
    {
        IEmailTemplate GetAddOnBuyEmailTemplate(string receipent, string stockId, string stockName, decimal newStopPrice, StockBuyState newState);

        IEmailTemplate GetBreakDownEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal lowPriceInStrategy, decimal currentLowPrice);

        IEmailTemplate GetBreakThroughEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal highPriceInStrategy, decimal currentHighPrice);

        IEmailTemplate GetDailyPriceImportFailEmailTemplate();

        IEmailTemplate GetHistoricalDataImportEmailTemplate(string receipent, IReadOnlyList<IHistoricalDataWaitingEntry> entries);

        IEmailTemplate GetStopLossEmailTemplate(string receipent, string stockId, string stockName, BuySellStrategyType strategy, decimal stopPriceInStrategy, decimal currentLowPrice);
    }
}