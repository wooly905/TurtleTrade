using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Infrastructure;
using TurtleTrade.Infrastructure.Storage;

namespace TurtleTrade.ServiceWorkers.LogisticWorkers
{
    public class CurrentPriceWorker : StockDataServiceWorker
    {
        public CurrentPriceWorker(IBaseData baseData)
            : base(baseData)
        {
            Name = "Current Price Worker";
            Kind = ServiceWorkerKind.CurrentPriceWorker;
            SetWorkerStartTimeEndTime(ServiceWorkerKind.CurrentPriceWorker);
            WriteToWorkerLog($"{Name} has been created");
        }

        protected override async Task RunInternalAsync(CancellationToken token)
        {
            WriteToHeartBeatLog();

            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IStock> _stockList = await DatabaseOperations.GetStocksAsync(Country).ConfigureAwait(false);

            if (_stockList == null
                || _stockList.Count == 0
                || token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IStockQuoteFromDataSource> priceData = await GetStockPricesFromDataSourceAsync(_stockList).ConfigureAwait(false);
            int successCount = 0;

            foreach (IStockQuoteFromDataSource data in priceData)
            {
                if (data.IsValid)
                {
                    ICurrentPrice currentPriceItem = new CurrentPriceItem(data.ClosePrice,
                                                                          data.TradeDateTime,
                                                                          data.HighPrice,
                                                                          data.LowPrice);

                    CountryKind country = data.Country.ConvertToTT2Country();
                    BaseData.CurrentPriceStorage.AddOrUpdateItem(country, data.StockId, currentPriceItem);
                    successCount++;
                }
            }

            WriteToWorkerLog($"Getting back {successCount} of {_stockList.Count} stocks from data source.");
        }
    }
}
