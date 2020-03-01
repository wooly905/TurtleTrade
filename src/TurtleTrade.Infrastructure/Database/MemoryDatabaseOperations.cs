using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    public class MemoryDatabaseOperations : IDatabaseOperations
    {
        public MemoryDatabaseOperations()
        {
            StockTable = new List<IStock>();
            StockPriceHistoryTable = new List<IStockPriceHistory>();
            MemberStockTable = new List<IMemberStock>();
            MemberBuyStockTable = new List<IMemberBuyStock>();
            HistoricalDataWaitingEntryTable = new List<IHistoricalDataWaitingEntry>();
        }

        #region not expose in interface
        public List<IStock> StockTable { get; }

        public List<IStockPriceHistory> StockPriceHistoryTable { get; }

        public List<IMemberStock> MemberStockTable { get; }

        public List<IMemberBuyStock> MemberBuyStockTable { get; }

        public List<IHistoricalDataWaitingEntry> HistoricalDataWaitingEntryTable { get; }

        #endregion

        public Task UpdateMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId, decimal stopPrice, StockBuyState newState)
        {
            IMemberBuyStock memberStock = MemberBuyStockTable.Find(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase) && a.Country == country);

            if (memberStock != null && memberStock is MemberBuyStock mStock)
            {
                mStock.StopPrice = stopPrice;
                mStock.State = newState;
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync()
        {
            return Task.FromResult<IReadOnlyList<IMemberStock>>(MemberStockTable);
        }

        public Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync(string memberEmail)
        {
            return Task.FromResult<IReadOnlyList<IMemberStock>>(MemberStockTable.Where(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(string memberEmail)
        {
            return Task.FromResult<IReadOnlyList<IMemberBuyStock>>(MemberBuyStockTable.Where(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(CountryKind country, StockBuyState state)
        {
            return Task.FromResult<IReadOnlyList<IMemberBuyStock>>(MemberBuyStockTable.Where(a => a.Country == country && a.State == state).ToList());
        }

        public Task SetMemberBuyStockBuyStateAsync(string memberEmail, CountryKind country, string stockID, StockBuyState buyState)
        {
            IMemberBuyStock stock = MemberBuyStockTable.FirstOrDefault(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)
                                                                            && a.Country == country
                                                                            && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase));

            if (!(stock is MemberBuyStock target))
            {
                return Task.CompletedTask;
            }

            target.State = buyState;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockID, DateTime start, DateTime end)
        {
            return Task.FromResult<IReadOnlyList<IStockPriceHistory>>(StockPriceHistoryTable.Where(a => a.Country == country
                                                                                                        && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase)
                                                                                                        && a.TradeDateTime >= start && a.TradeDateTime <= end).ToList());
        }

        public Task<IReadOnlyList<IStock>> GetStocksAsync(CountryKind country)
        {
            return Task.FromResult<IReadOnlyList<IStock>>(StockTable.Where(a => a.Country == country).ToList());
        }

        public Task<IStockPriceHistory> GetStockPriceHistoryAsync(CountryKind country, string stockID, DateTime date)
        {
            IStockPriceHistory priceHistory = StockPriceHistoryTable.Find(a => a.Country == country
                                                                               && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase)
                                                                               && a.TradeDateTime.Year == date.Year
                                                                               && a.TradeDateTime.Month == date.Month
                                                                               && a.TradeDateTime.Day == date.Day);

            return Task.FromResult(priceHistory);
        }

        public Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockID, DateTime dateBefore, int recordNumber)
        {
            return Task.FromResult<IReadOnlyList<IStockPriceHistory>>(StockPriceHistoryTable.Where(a => a.Country == country
                                                                                                   && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase)
                                                                                                   && a.TradeDateTime < dateBefore)
                                                                                            .OrderByDescending(a => a.TradeDateTime)
                                                                                            .Take(recordNumber)
                                                                                            .ToList());
        }

        public Task AddOrUpdateStockPriceHistoryAsync(IStockPriceHistory data)
        {
            IStockPriceHistory priceHistory = StockPriceHistoryTable.FirstOrDefault(a => a.Country == data.Country
                                                                                         && string.Equals(a.StockId, data.StockId, StringComparison.OrdinalIgnoreCase)
                                                                                         && a.TradeDateTime.Year == data.TradeDateTime.Year
                                                                                         && a.TradeDateTime.Month == data.TradeDateTime.Month
                                                                                         && a.TradeDateTime.Day == data.TradeDateTime.Day);

            switch (priceHistory)
            {
                case null:
                    StockPriceHistoryTable.Add(data);
                    return Task.CompletedTask;
                case StockPriceHistory stockPriceHistory:
                    stockPriceHistory.ATR = data.ATR;
                    stockPriceHistory.ClosePrice = data.ClosePrice;
                    stockPriceHistory.HighIn20 = data.HighIn20;
                    stockPriceHistory.HighPrice = data.HighPrice;
                    stockPriceHistory.LowIn10 = data.LowIn10;
                    stockPriceHistory.LowPrice = data.LowPrice;
                    stockPriceHistory.MA120 = data.MA120;
                    stockPriceHistory.MA20 = data.MA20;
                    stockPriceHistory.MA240 = data.MA240;
                    stockPriceHistory.MA60 = data.MA60;
                    stockPriceHistory.N20 = data.N20;
                    stockPriceHistory.OpenPrice = data.OpenPrice;
                    stockPriceHistory.PriceChange = data.PriceChange;
                    stockPriceHistory.PriceRange = data.PriceRange;
                    stockPriceHistory.Volume = data.Volume;
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<IAllPricesEntry> GetTheLatestStockPriceAsync(CountryKind country, string stockID, DateTime date)
        {
            IStockPriceHistory priceHistory = StockPriceHistoryTable.OrderByDescending(a => a.TradeDateTime)
                                                                    .FirstOrDefault(a => a.Country == country
                                                                                         && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase)
                                                                                         && a.TradeDateTime < date);

            if (priceHistory == null)
            {
                return Task.FromResult<IAllPricesEntry>(null);
            }

            AllPricesEntry entry = new AllPricesEntry(country,
                                                      stockID,
                                                      priceHistory.LowPrice,
                                                      priceHistory.HighPrice,
                                                      priceHistory.ClosePrice,
                                                      priceHistory.OpenPrice,
                                                      priceHistory.TradeDateTime,
                                                      priceHistory.YearRange,
                                                      priceHistory.Volume,
                                                      priceHistory.ATR,
                                                      priceHistory.N20,
                                                      priceHistory.HighIn20,
                                                      priceHistory.LowIn10,
                                                      priceHistory.N40,
                                                      priceHistory.HighIn40,
                                                      priceHistory.LowIn15,
                                                      priceHistory.N60,
                                                      priceHistory.HighIn60,
                                                      priceHistory.LowIn20,
                                                      priceHistory.MA20,
                                                      priceHistory.MA40,
                                                      priceHistory.MA60,
                                                      priceHistory.MA120,
                                                      priceHistory.MA240);

            return Task.FromResult<IAllPricesEntry>(entry);
        }

        public Task<IReadOnlyList<IAllPricesEntry>> GetTheLatestStockPriceAsync(CountryKind country, DateTime date)
        {
            IReadOnlyList<IStockPriceHistory> priceHistories = StockPriceHistoryTable.Where(a => a.Country == country && a.TradeDateTime < date)
                                                                                     .OrderByDescending(a => a.TradeDateTime)
                                                                                     .ToList();

            if (priceHistories.Count == 0)
            {
                return Task.FromResult<IReadOnlyList<IAllPricesEntry>>(null);
            }

            List<IAllPricesEntry> list = priceHistories.Select(history => new AllPricesEntry(country,
                                                                                             history.StockId,
                                                                                             history.LowPrice,
                                                                                             history.HighPrice,
                                                                                             history.ClosePrice,
                                                                                             history.OpenPrice,
                                                                                             history.TradeDateTime,
                                                                                             history.YearRange,
                                                                                             history.Volume,
                                                                                             history.ATR,
                                                                                             history.N20,
                                                                                             history.HighIn20,
                                                                                             history.LowIn10,
                                                                                             history.N40,
                                                                                             history.HighIn40,
                                                                                             history.LowIn15,
                                                                                             history.N60,
                                                                                             history.HighIn60,
                                                                                             history.LowIn20,
                                                                                             history.MA20,
                                                                                             history.MA40,
                                                                                             history.MA60,
                                                                                             history.MA120,
                                                                                             history.MA240))
                                                      .Cast<IAllPricesEntry>()
                                                      .ToList();

            return Task.FromResult<IReadOnlyList<IAllPricesEntry>>(list);
        }

        public Task AddStockAsync(CountryKind country, string stockId, string stockName, string stockExchangeName, string stockDescription, string stockExchangeID)
        {
            IEnumerable<IStock> stocks = StockTable.Where(a => a.Country == country
                                                               && string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            if (stocks.Any())
            {
                return Task.CompletedTask;
            }

            Stock stock = new Stock(country, stockId, stockName, stockExchangeID)
            {
                StockExchangeName = stockExchangeName,
                Description = stockDescription
            };

            StockTable.Add(stock);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockID)
        {
            return Task.FromResult<IReadOnlyList<IStockPriceHistory>>(StockPriceHistoryTable.Where(a => a.Country == country && string.Equals(a.StockId, stockID, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync()
        {
            return Task.FromResult<IReadOnlyList<IMemberBuyStock>>(MemberBuyStockTable);
        }

        public Task<IReadOnlyList<IHistoricalDataWaitingEntry>> GetWaitingEntriesAsync(CountryKind country)
        {
            IReadOnlyList<IHistoricalDataWaitingEntry> waitingEntries = HistoricalDataWaitingEntryTable.Where(a => a.State == HistoricalDataWaitingState.Waiting && a.Country == country).ToList();

            return Task.FromResult(waitingEntries);
        }

        public Task SetWaitingEntryToWorkingAsync(CountryKind country, string stockId)
        {
            return SetWaitingEntryToStateInternalAsync(country, stockId, HistoricalDataWaitingState.Working);
        }

        private Task SetWaitingEntryToStateInternalAsync(CountryKind country, string stockId, HistoricalDataWaitingState state)
        {
            IHistoricalDataWaitingEntry entry = HistoricalDataWaitingEntryTable.Find(a => a.Country == country &&
                                                                                          string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            if (entry is HistoricalDataWaitingEntry tempEntry)
            {
                tempEntry.State = state;
            }

            return Task.CompletedTask;
        }

        public Task SetWaitingEntryToDoneAsync(CountryKind country, string stockId)
        {
            return SetWaitingEntryToStateInternalAsync(country, stockId, HistoricalDataWaitingState.Done);
        }

        public Task DeleteDoneWaitingEntriesAsync()
        {
            // need to create a new list so that entry can be removed in for loop.
            IList<IHistoricalDataWaitingEntry> entries = HistoricalDataWaitingEntryTable.Where(a => a.State == HistoricalDataWaitingState.Done).ToList();

            foreach (IHistoricalDataWaitingEntry entry in entries)
            {
                HistoricalDataWaitingEntryTable.Remove(entry);
            }

            return Task.CompletedTask;
        }

        public Task AddWaitingEntryAsync(CountryKind country, string stockId, DateTime startDate, DateTime endDate)
        {
            IHistoricalDataWaitingEntry entry = HistoricalDataWaitingEntryTable.Find(a => a.Country == country
                                                                                          && string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            if (entry != null)
            {
                return Task.CompletedTask;
            }

            entry = new HistoricalDataWaitingEntry(country, stockId, HistoricalDataWaitingState.Waiting, startDate, endDate);
            HistoricalDataWaitingEntryTable.Add(entry);

            return Task.CompletedTask;
        }

        public Task DeleteStockPriceHistoryAsync(CountryKind country, string stockId)
        {
            IEnumerable<IStockPriceHistory> entries = StockPriceHistoryTable.Where(a => a.Country == country &&
                                                                                        string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            foreach (IStockPriceHistory item in entries)
            {
                StockPriceHistoryTable.Remove(item);
            }

            return Task.CompletedTask;
        }

        public Task DeleteMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId)
        {
            IEnumerable<IMemberBuyStock> items = MemberBuyStockTable.Where(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)
                                                                                && a.Country == country
                                                                                && string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            if (!items.Any())
            {
                return Task.CompletedTask;
            }

            foreach (IMemberBuyStock item in items)
            {
                MemberBuyStockTable.Remove(item);
            }

            return Task.CompletedTask;
        }

        public Task AddMemberBuyStockAsync(string memberEmail, CountryKind country, string stockID, decimal buyPrice, decimal N, BuySellStrategyType strategy, DateTime buyDate)
        {
            IMemberBuyStock memberBuyStock = new MemberBuyStock(memberEmail, country, stockID, buyPrice, N, buyPrice - (2 * N), StockBuyState.Buy, strategy, buyDate);
            MemberBuyStockTable.Add(memberBuyStock);

            return Task.CompletedTask;
        }

        public Task AddMemberStockAsync(string memberEmail, CountryKind country, string stockID, BuySellStrategyType strategy, bool isNotify)
        {
            MemberStock stock = new MemberStock(memberEmail, country, stockID, isNotify, strategy);
            MemberStockTable.Add(stock);

            return Task.CompletedTask;
        }

        public Task SetMemberStockAsync(string memberEmail, CountryKind country, string stockId, BuySellStrategyType strategy, bool isNotify)
        {
            IMemberStock stock = MemberStockTable.Find(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)
                                                            && a.Country == country
                                                            && string.Equals(a.StockId, stockId, StringComparison.OrdinalIgnoreCase));

            if (!(stock is MemberStock target))
            {
                return Task.CompletedTask;
            }

            target.Strategy = strategy;
            target.IsNotify = isNotify;

            return Task.CompletedTask;
        }
    }
}
