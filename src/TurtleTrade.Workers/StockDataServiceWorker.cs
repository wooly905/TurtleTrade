using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Infrastructure;

namespace TurtleTrade.ServiceWorkers
{
    public abstract class StockDataServiceWorker : ServiceWorker
    {
        protected StockDataServiceWorker(IBaseData baseData)
            : base(baseData)
        {
        }

        protected IDatabaseOperations DatabaseOperations => BaseData.GetDatabaseOperations();

        protected IStockQuoteDataSource YahooDataSource => BaseData.GetStockDataSources().FirstOrDefault(a => a.Source == StockQuoteSource.Yahoo);

        private async Task<IStockQuoteFromDataSource> GetStockPricesFromDataSourceInternalAsync(CountryKind country, string stockId)
        {

            // TODO: 3 moves to constant ?
            for (int retry = 0; retry < 3; retry++)
            {
                foreach (IStockQuoteDataSource source in BaseData.GetStockDataSources())
                {
                    IStockQuoteFromDataSource stockData = null;

                    try
                    {
                        stockData = await source.GetMostRecentQuoteAsync(country.ConvertToTTStockQuoteSourceCountry(),
                                                                         stockId,
                                                                         WriteToErrorLog).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    if (TestStatus && (stockData?.IsValid != true))
                    {
                        continue;
                    }

                    if (!TestStatus
                        && (stockData?.IsValid != true || !BaseData.CurrentTime.IsSameDay(stockData.TradeDateTime)))
                    {
                        continue;
                    }

                    return stockData;
                }
            }

            return null;
        }

        protected async Task<IReadOnlyList<IStockQuoteFromDataSource>> GetStockPricesFromDataSourceAsync(IReadOnlyList<IStock> stocks)
        {
            if (stocks == null || stocks.Count == 0)
            {
                return null;
            }

            Task<IStockQuoteFromDataSource>[] tasks = new Task<IStockQuoteFromDataSource>[stocks.Count];

            for (int i = 0; i < stocks.Count; i++)
            {
                tasks[i] = GetStockPricesFromDataSourceInternalAsync(stocks[i].Country, stocks[i].StockId);
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(ex);
            }

            List<IStockQuoteFromDataSource> result = new List<IStockQuoteFromDataSource>();

            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i].Status == TaskStatus.RanToCompletion && tasks[i].Result != null)
                {
                    result.Add(tasks[i].Result);
                }
            }

            return result;
        }
    }
}
