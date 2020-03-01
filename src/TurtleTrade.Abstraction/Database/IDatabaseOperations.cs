using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurtleTrade.Abstraction.Database
{
    public interface IDatabaseOperations
    {
        // member
        Task AddMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId, decimal buyPrice, decimal N, BuySellStrategyType strategy, DateTime buyDate);
        Task AddMemberStockAsync(string memberEmail, CountryKind country, string stockId, BuySellStrategyType strategy, bool isNotify);
        Task DeleteMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId);
        Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync();
        Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(string memberEmail);
        Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(CountryKind country, StockBuyState state);
        Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync();
        Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync(string memberEmail);
        Task SetMemberBuyStockBuyStateAsync(string memberEmail, CountryKind country, string stockId, StockBuyState buyState);
        Task SetMemberStockAsync(string memberEmail, CountryKind country, string stockId, BuySellStrategyType strategy, bool isNotify);
        Task UpdateMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId, decimal stopPrice, StockBuyState newState);

        // stock
        Task<IReadOnlyList<IStock>> GetStocksAsync(CountryKind country);
        Task<IStockPriceHistory> GetStockPriceHistoryAsync(CountryKind country, string stockId, DateTime date);
        Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockId);
        Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockId, DateTime start, DateTime end);
        Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockId, DateTime dateBefore, int recordNumber);
        Task AddOrUpdateStockPriceHistoryAsync(IStockPriceHistory data);
        Task AddStockAsync(CountryKind country, string stockId, string stockName, string stockExchangeName, string stockDescription, string stockExchangeId);
        Task<IAllPricesEntry> GetTheLatestStockPriceAsync(CountryKind country, string stockId, DateTime date);
        Task<IReadOnlyList<IAllPricesEntry>> GetTheLatestStockPriceAsync(CountryKind country, DateTime date);
        Task DeleteStockPriceHistoryAsync(CountryKind country, string stockId);

        // historical data waiting list
        Task AddWaitingEntryAsync(CountryKind country, string stockId, DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<IHistoricalDataWaitingEntry>> GetWaitingEntriesAsync(CountryKind country);
        Task SetWaitingEntryToWorkingAsync(CountryKind country, string stockId);
        Task SetWaitingEntryToDoneAsync(CountryKind country, string stockId);
        Task DeleteDoneWaitingEntriesAsync();
    }
}
