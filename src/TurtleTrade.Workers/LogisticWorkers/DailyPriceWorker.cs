using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure;
using TurtleTrade.Infrastructure.EmailTemplates;

namespace TurtleTrade.ServiceWorkers.LogisticWorkers
{
    public class DailyPriceWorker : StockDataServiceWorker
    {
        private readonly string _adminEmail;

        public DailyPriceWorker(IBaseData baseData)
            : base(baseData)
        {
            Name = "Daily Price Worker";
            Kind = ServiceWorkerKind.DailyPriceWorker;
            SetWorkerStartTimeEndTime(ServiceWorkerKind.DailyPriceWorker);
            _adminEmail = SystemConfig.SystemInfo.AdminEmail;
            WriteToWorkerLog($"{Name} has been created");
        }

        protected override async Task RunInternalAsync(CancellationToken token)
        {
            WriteToHeartBeatLog();

            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IStock> stocks = await DatabaseOperations.GetStocksAsync(Country).ConfigureAwait(false);

            if (stocks == null || stocks.Count == 0)
            {
                WriteToWorkerLog("No stock to import!");
                return;
            }

            List<(string, string)> importFailedStocks = new List<(string, string)>();
            StockPriceHistoryInsertion insertion = new StockPriceHistoryInsertion(BaseData, DatabaseOperations);
            IReadOnlyList<IStockQuoteFromDataSource> stockQuotes = await GetStockPricesFromDataSourceAsync(stocks).ConfigureAwait(false);

            if (stockQuotes == null || stockQuotes.Count == 0)
            {
                WriteToWorkerLog("No stock quote data comes back from data source");
                return;
            }

            List<IStock> failedStocks = stocks.ToList();
            int successCount = 0;

            foreach (IStockQuoteFromDataSource stockQuote in stockQuotes)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (!stockQuote.IsValid)
                {
                    continue;
                }

                try
                {
                    await insertion.InsertToDatabase(stockQuote).ConfigureAwait(false);
                    RemoveStockQuote(failedStocks, stockQuote);
                    successCount++;
                }
                catch (Exception ex)
                {
                    WriteToErrorLog(ex);
                    importFailedStocks.Add(($"{stockQuote.Country.ConvertToTT2Country().GetShortName()}.{stockQuote.StockId})", ex.ToString()));
                    // TODO : 想一想，如果失敗了，如何自動重新再試一次 -> how about OperationWorker
                }
            }

            WriteToWorkerLog($"Daily stock data import has been completed. Processed # of stock = {successCount}/{stocks.Count}");

            if (token.IsCancellationRequested)
            {
                return;
            }

            foreach (IStock stock in failedStocks)
            {
                // BUG : may have duplicate item potentially
                importFailedStocks.Add(($"{stock.Country.GetShortName()}.{stock.StockId} {stock.StockName}", "Failed to insert to database"));
            }

            SendFailedImportNotification(importFailedStocks);
        }

        private void RemoveStockQuote(List<IStock> stocks, IStockQuoteFromDataSource stockQuote)
        {
            IStock stock = stocks.Find(x => string.Equals(x.StockId, stockQuote.StockId, StringComparison.OrdinalIgnoreCase));

            if (stock != null)
            {
                stocks.Remove(stock);
            }
        }

        private void SendFailedImportNotification(IList<(string, string)> failedData)
        {
            if (TestStatus)
            {
                return;
            }

            // TODO : should use template provider to generate email template object
            IEmailTemplate emailBody = new DailyPriceImportFailEmailTemplate(_adminEmail,
                                                                             failedData,
                                                                             "Turtle2 - DailyPriceImport Notification, Country = " + BaseData.Country.GetShortName());

            EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailBody);
        }
    }
}