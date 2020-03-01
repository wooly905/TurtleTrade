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
    // This worker will run between 0:0 and 23:59

    public class HistoricalPriceWorker : StockDataServiceWorker
    {
        private readonly IEmailTemplateProvider _emailTemplateProvider;

        public HistoricalPriceWorker(IBaseData baseData)
            : base(baseData)
        {
            Name = "Historical Price Worker";
            Kind = ServiceWorkerKind.HistoricalPriceWorker;
            WriteToWorkerLog($"{Name} has been created");

            // TODO : move to other place
            _emailTemplateProvider = new EmailTemplateProvider();
        }

        protected override async Task RunInternalAsync(CancellationToken token)
        {
            WriteToHeartBeatLog();

            if (token.IsCancellationRequested)
            {
                return;
            }

            // read databae historical data table
            IReadOnlyList<IHistoricalDataWaitingEntry> waitingEntries = await GetStockEntries(BaseData.Country).ConfigureAwait(false);

            if (waitingEntries == null || waitingEntries.Count == 0)
            {
                return;
            }

            StockPriceHistoryInsertion insertion = new StockPriceHistoryInsertion(BaseData, DatabaseOperations);

            foreach (IHistoricalDataWaitingEntry entry in waitingEntries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // set state to working in historical table
                await DatabaseOperations.SetWaitingEntryToWorkingAsync(entry.Country, entry.StockId).ConfigureAwait(false);
                //Task.Run(() => CoreRunAsync(entry, insertion)); // make azure db too busy
                await CoreRunAsync(entry, insertion, token).ConfigureAwait(false);
            }

            await EmailService.SendEmailAsync(Country,
                                              CurrentTime,
                                              _emailTemplateProvider.GetHistoricalDataImportEmailTemplate(SystemConfig.SystemInfo.AdminEmail, waitingEntries))
                                             .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<IHistoricalDataWaitingEntry>> GetStockEntries(CountryKind country)
        {
            DateTime? historicalPriceImportTime = SystemConfig.TradingTimes.FirstOrDefault(c => c.Country == country)?.HistoricalPriceImportTime;

            if (historicalPriceImportTime == null)
            {
                return null;
            }

            if (BaseData.CurrentTime.DayOfWeek == DayOfWeek.Sunday
                && BaseData.CurrentTime.Hour == historicalPriceImportTime.Value.Hour
                && BaseData.CurrentTime.Minute == historicalPriceImportTime.Value.Minute)
            {
                IReadOnlyList<IStock> stocks = await DatabaseOperations.GetStocksAsync(country).ConfigureAwait(false);
                List<IHistoricalDataWaitingEntry> entries = new List<IHistoricalDataWaitingEntry>();

                // we always get historical data which starts from 2014/1/1
                DateTime startDate = new DateTime(2014, 1, 1, 0, 0, 0);

                foreach (IStock stock in stocks)
                {
                    HistoricalStockEntry entry = new HistoricalStockEntry(stock, startDate, BaseData.CurrentTime);
                    entries.Add(entry);
                }

                return entries;
            }

            return await DatabaseOperations.GetWaitingEntriesAsync(BaseData.Country).ConfigureAwait(false);
        }

        private async Task CoreRunAsync(IHistoricalDataWaitingEntry entry, StockPriceHistoryInsertion insertion, CancellationToken token)
        {
            DateTime start = DateTime.Now;
            WriteToWorkerLog($"Start {entry.StockId}");

            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IStockQuoteFromDataSource> records = await YahooDataSource.GetHistoricalQuotesAsync(entry.Country.ConvertToTTStockQuoteSourceCountry(), 
                                                                                                              entry.StockId,
                                                                                                              entry.DataStartDate,
                                                                                                              entry.DataEndDate,
                                                                                                              WriteToErrorLog)
                                                                                    .ConfigureAwait(false);

            if (records == null || records.Count == 0)
            {
                WriteToWorkerLog($"Data source returned NULL historical data of {entry.Country.GetShortName()}.{entry.StockId}");
                return;
            }

            foreach (IStockQuoteFromDataSource record in records)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // insert records into database
                await insertion.InsertToDatabase(record).ConfigureAwait(false);
            }

            await DatabaseOperations.SetWaitingEntryToDoneAsync(entry.Country, entry.StockId).ConfigureAwait(false);

            TimeSpan timeSpan = DateTime.Now - start;
            WriteToWorkerLog($"Finish {entry.Country.GetShortName()}.{entry.StockId}. Total time = {timeSpan.TotalSeconds} seconds");
        }
    }

    internal class HistoricalStockEntry : IHistoricalDataWaitingEntry
    {
        public HistoricalStockEntry(IStock stock, DateTime startDate, DateTime endDate)
        {
            State = HistoricalDataWaitingState.Waiting;
            Country = stock.Country;
            StockId = stock.StockId;
            DataStartDate = startDate;
            DataEndDate = endDate;
        }

        public HistoricalDataWaitingState State { get; internal set; }

        public DateTime DataStartDate { get; internal set; }

        public DateTime DataEndDate { get; internal set; }

        public CountryKind Country { get; internal set; }

        public string StockId { get; internal set; }
    }
}
