using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Database;

[assembly: InternalsVisibleTo("Turtle2.ServiceWorkers")]
[assembly: InternalsVisibleTo("Turtle2.Test")]

namespace TurtleTrade.Infrastructure
{
    internal class StockPriceHistoryInsertion
    {
        private IBaseData _baseData;
        private IDatabaseOperations _database;
        private const int _maxCalculationDay = 240;

        public StockPriceHistoryInsertion(IBaseData baseData, IDatabaseOperations database)
        {
            _baseData = baseData;
            _database = database;
        }

        public async Task InsertToDatabase(IStockQuoteFromDataSource stockData)
        {
            // get previous close price , if none, it's the first record and then insert to db
            IReadOnlyList<IStockPriceHistory> previousPrices = await _database.GetStockPriceHistoryAsync(stockData.Country.ConvertToTT2Country(), stockData.StockId, stockData.TradeDateTime, 1).ConfigureAwait(false);

            IStockPriceHistory previousStockPriceHistory = previousPrices?.FirstOrDefault();

            if (previousStockPriceHistory == null)
            {
                //string msg = $"The stock data {GetStockFullID(todayPriceFromDataSource.StockId)} from data sourceKind doesn't have the previous closed price, so it will NOT be added to database";
                //WriteToErrorLog(msg);
                IStockPriceHistory priceHist = new StockPriceHistory(stockData.Country.ConvertToTT2Country(), stockData.StockId, stockData.TradeDateTime)
                {
                    OpenPrice = stockData.OpenPrice,
                    ClosePrice = stockData.ClosePrice,
                    HighPrice = stockData.HighPrice,
                    LowPrice = stockData.LowPrice,
                    Volume = stockData.Volume
                };

                await _database.AddOrUpdateStockPriceHistoryAsync(priceHist).ConfigureAwait(false);
                return;
            }

            // get the max days of SystemN
            IReadOnlyList<IStockPriceHistory> recentStockPriceHistory = await _database.GetStockPriceHistoryAsync(stockData.Country.ConvertToTT2Country(), stockData.StockId, stockData.TradeDateTime, _maxCalculationDay - 1).ConfigureAwait(false);

            // if there exists previous closed price, then calculate today's ATR
            decimal todayATR = _baseData.CalculateTodayATR(stockData.HighPrice, stockData.LowPrice, previousStockPriceHistory.ClosePrice);

            (decimal? N20, decimal? HighIn20, decimal? LowIn10) = DailyPriceCalculationSystem(BuySellStrategyType.N20, recentStockPriceHistory, todayATR, stockData);
            (decimal? N40, decimal? HighIn40, decimal? LowIn15) = DailyPriceCalculationSystem(BuySellStrategyType.N40, recentStockPriceHistory, todayATR, stockData);
            (decimal? N60, decimal? HighIn60, decimal? LowIn20) = DailyPriceCalculationSystem(BuySellStrategyType.N60, recentStockPriceHistory, todayATR, stockData);
            decimal? MA20 = CalculateMovingAverage(20, recentStockPriceHistory, stockData);
            decimal? MA40 = CalculateMovingAverage(40, recentStockPriceHistory, stockData);
            decimal? MA60 = CalculateMovingAverage(60, recentStockPriceHistory, stockData);
            decimal? MA120 = CalculateMovingAverage(120, recentStockPriceHistory, stockData);
            decimal? MA240 = CalculateMovingAverage(240, recentStockPriceHistory, stockData);

            IStockPriceHistory priceHistory = new StockPriceHistory(stockData.Country.ConvertToTT2Country(), stockData.StockId, stockData.TradeDateTime)
            {
                OpenPrice = stockData.OpenPrice,
                ClosePrice = stockData.ClosePrice,
                HighPrice = stockData.HighPrice,
                LowPrice = stockData.LowPrice,
                Volume = stockData.Volume,
                ATR = todayATR,
                N20 = N20,
                N40 = N40,
                N60 = N60,
                HighIn20 = HighIn20,
                LowIn10 = LowIn10,
                HighIn40 = HighIn40,
                LowIn15 = LowIn15,
                HighIn60 = HighIn60,
                LowIn20 = LowIn20,
                MA20 = MA20,
                MA40 = MA40,
                MA60 = MA60,
                MA120 = MA120,
                MA240 = MA240
            };

            await _database.AddOrUpdateStockPriceHistoryAsync(priceHistory).ConfigureAwait(false);
        }

        private (decimal? NInSystem, decimal? highestPriceInSystem, decimal? lowestPriceInSystem) DailyPriceCalculationSystem(BuySellStrategyType type, IReadOnlyList<IStockPriceHistory> recentStockPriceHistory, decimal todayATR, IStockQuoteFromDataSource todayPriceDataFromDataSource)
        {
            decimal[] lowPrices = null;
            decimal[] highPrices = null;
            decimal?[] ATRs = null;
            decimal? lowestPrice = null;
            decimal? highestPrice = null;
            decimal? todayN = null;

            if (recentStockPriceHistory.Count >= type.GetSellIntValue())
            {
                lowPrices = recentStockPriceHistory.Take(type.GetSellIntValue()).Select(a => a.LowPrice).ToArray();
                ATRs = recentStockPriceHistory.Take(type.GetSellIntValue() - 1).Select(a => a.ATR).ToArray();
            }
            if (recentStockPriceHistory.Count() >= type.GetBuyIntValue())
            {
                highPrices = recentStockPriceHistory.Take(type.GetBuyIntValue()).Select(a => a.HighPrice).ToArray();
                ATRs = recentStockPriceHistory.Take(type.GetBuyIntValue() - 1).Select(a => a.ATR).ToArray();
            }

            if (lowPrices != null)
            {
                lowestPrice = _baseData.CalculateLowestPrice(lowPrices, todayPriceDataFromDataSource.LowPrice);
            }

            if (highPrices != null)
            {
                highestPrice = _baseData.CalculateHighestPrice(highPrices, todayPriceDataFromDataSource.HighPrice);
            }

            if (ATRs != null)
            {
                List<decimal> validATRList = new List<decimal>();

                foreach (decimal? item in ATRs)
                {
                    if (item.HasValue)
                    {
                        validATRList.Add(item.Value);
                    }
                }

                if (validATRList.Count() == type.GetBuyIntValue() - 1)
                {
                    todayN = _baseData.CalculateTodayN(validATRList.ToArray(), todayATR);
                }
            }

            return (todayN, highestPrice, lowestPrice);
        }

        private decimal? CalculateMovingAverage(int days, IReadOnlyList<IStockPriceHistory> recentStockPriceHistory, IStockQuoteFromDataSource todayPriceDataFromDataSource)
        {
            if (recentStockPriceHistory.Count() >= days - 1)
            {
                decimal[] closePrices = recentStockPriceHistory.Take(days - 1).Select(a => a.ClosePrice).ToArray();
                return (closePrices.Sum() + todayPriceDataFromDataSource.ClosePrice) / (decimal)days;
            }

            return null;
        }
    }
}
