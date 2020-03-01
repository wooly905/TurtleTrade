using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Database;
using TurtleTrade.Infrastructure.Storage;
using TurtleTrade.ServiceWorkers.BuySellStrategy;

namespace TurtleTrade.ServiceWorkers.BackTestWorkers
{
    // TODO : BackTest should be like Worker not Strategy
    internal class BackTestWorker : ServiceWorker
    {
        private readonly IBaseData _baseData;
        private readonly string _stockId;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly ICurrentPriceStorage _storage;
        private readonly IDatabaseOperations _turtleDbOperations;
        private readonly IDatabaseOperations _memoryDbOperations;
        private readonly BackTestNotificationService _notificationService;
        private readonly BackTestNotificationTemplateProvider _notificationTemplateProvider;
        private readonly IStockPriceNotificationChecker _stockPriceNotificationChecker;
        private readonly Action<string> _writeToLogAction;
        private readonly Action<Exception> _writeToErrorLogAction;
        private readonly Task _prepareMemoryDbTask;
        private readonly string _dummyEmail = "test@gmail.com";
        private ITurtleLogger _logger;

        public BackTestWorker(IBaseData baseData, string stockId, Action<string> writeToLogAction, Action<Exception> writeToErrorLogAction, bool alwaysRun = true)
            : base(baseData)
        {
            _baseData = baseData;
            _stockId = stockId;

            _startDate = baseData.CurrentTime;
            _endDate = DateTime.Now;

            _writeToLogAction = writeToLogAction;
            _writeToErrorLogAction = writeToErrorLogAction;

            // create StorageSpace
            _storage = new CurrentPriceStorage();

            // create Turtle Database
            _turtleDbOperations = new TurtleDatabaseOperations(baseData.SystemConfig.SystemInfo.ProductionTurtleDBConnectionString);
            _memoryDbOperations = new MemoryDatabaseOperations();
            _prepareMemoryDbTask = Task.Run(() => PrepareMemoryDatabaseAsync());

            // 
            _stockPriceNotificationChecker = BaseData.CreateStockPriceNotificationChecker();

            // create EmailService and EmailTemplateProvider
            _logger = BaseData.GetLogger();
            _notificationService = new BackTestNotificationService(_logger);
            _notificationTemplateProvider = new BackTestNotificationTemplateProvider();
        }

        private async Task<bool> UpdatePriceInStorageAsync(DateTime currentDate)
        {
            IStockPriceHistory item = await _memoryDbOperations.GetStockPriceHistoryAsync(_baseData.Country, _stockId, currentDate).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            ICurrentPrice value = new CurrentPriceItem(item.ClosePrice, item.TradeDateTime, item.HighPrice, item.LowPrice);
            _storage.AddOrUpdateItem(_baseData.Country, _stockId, value);

            return true;
        }

        private async Task PrepareMemoryDatabaseAsync()
        {
            // copy stock price history from turtle db to memory db
            IReadOnlyList<IStockPriceHistory> items = await _turtleDbOperations.GetStockPriceHistoryAsync(_baseData.Country, _stockId, _startDate, _endDate).ConfigureAwait(false);

            if (items != null)
            {
                IList<IStockPriceHistory> list = items.ToList();
                foreach (IStockPriceHistory item in list)
                {
                    await _memoryDbOperations.AddOrUpdateStockPriceHistoryAsync(item);
                }
            }
        }

        public IReadOnlyList<BackTestBuySellRecord> GetResult()
        {
            return _notificationService.GetBuySellReocrds();
        }

        public string GenerateReport(IReadOnlyList<BackTestBuySellRecord> records, int principle)
        {
            if (records == null || records.Count == 0 || principle <= 0)
            {
                return null;
            }

            bool isBuying = false;
            decimal buyPrice = 0m;
            decimal finalMoney = 0m;
            int winNumber = 0;
            int loseNumber = 0;
            string display;
            string stockId = records.FirstOrDefault()?.StockId;

            StringBuilder sb = new StringBuilder();

            foreach (BackTestBuySellRecord record in records)
            {
                if (string.Equals(record.Action, "sell", StringComparison.OrdinalIgnoreCase) && isBuying)
                {
                    isBuying = false;
                    decimal money = Math.Round(principle / buyPrice) * (record.Price - buyPrice);

                    if (money > 0m)
                    {
                        winNumber++;
                    }
                    else
                    {
                        loseNumber++;
                    }

                    finalMoney += money;
                    display = string.Format("{0,12} , {1,12} , {2,12} , {3,12} , {4,12}, {5}",
                                            record.Date.ToString("yyyy/MM/dd"),
                                            record.Action,
                                            record.Op,
                                            record.Price,
                                            money,
                                            record.Comment);
                    sb.AppendLine(display);

                    continue;
                }

                if (string.Equals(record.Action, "buy", StringComparison.OrdinalIgnoreCase) && !isBuying)
                {
                    isBuying = true;
                    buyPrice = record.Price;
                }

                display = string.Format("{0,12} , {1,12} , {2,12} , {3,12} , {4,12}, {5}",
                                        record.Date.ToString("yyyy/MM/dd"),
                                        record.Action,
                                        record.Op,
                                        record.Price,
                                        "",
                                        record.Comment);
                sb.AppendLine(display);
            }

            sb.AppendLine("");
            sb.AppendFormat("Win % = {0:P1} , final money = {1}",
                            winNumber / (decimal)(winNumber + loseNumber),
                            finalMoney).AppendLine();

            if (records.Count > 0)
            {
                string currentPath = Environment.CurrentDirectory;
                string filename = string.IsNullOrEmpty(stockId) ? "No-Stock-id" : stockId.ToUpper() + ".csv";
                string filePath = Path.Combine(currentPath, filename);

                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, false))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _writeToErrorLogAction(ex);
                }

                return filePath;
            }

            return null;
        }

        protected override async Task RunInternalAsync(CancellationToken token)
        {
            if (_prepareMemoryDbTask.Status == TaskStatus.Canceled || _prepareMemoryDbTask.Status == TaskStatus.Faulted)
            {
                return;
            }

            while (true)
            {
                await Task.Delay(300).ConfigureAwait(false);

                if (_prepareMemoryDbTask.Status == TaskStatus.RanToCompletion)
                {
                    break;
                }
                else if (_prepareMemoryDbTask.Status == TaskStatus.Canceled || _prepareMemoryDbTask.Status == TaskStatus.Faulted)
                {
                    return;
                }
            }

            await _memoryDbOperations.AddMemberStockAsync(_dummyEmail,
                                                          _baseData.Country,
                                                          _stockId,
                                                          BuySellStrategyType.N20, true).ConfigureAwait(false);

            for (DateTime currentDate = _startDate; currentDate < _endDate; currentDate = currentDate.AddDays(1))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // change date
                _notificationTemplateProvider.SetDateTime(currentDate);

                // update price in storage
                bool updateResult = await UpdatePriceInStorageAsync(currentDate).ConfigureAwait(false);

                if (!updateResult)
                {
                    continue;
                }

                Console.WriteLine($"Working on {currentDate.ToString("yyyy-MM-dd")}");

                // create sell operation and run 
                // 先跑 sell 以避免停損價在盤中被BuyWorker 提升造成錯誤賣出的問題 以及 買了隔天才能再賣
                await SellOperationInternal(token).ConfigureAwait(false);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                // buy operation
                await BuyOperationInternal(token).ConfigureAwait(false);

                return;
            }

            return;
        }

        private async Task BuyOperationInternal(CancellationToken token)
        {
            IMemberStock memberStock = new MemberStock(_dummyEmail, _baseData.Country, _stockId, true);

            // create BuyRunner and run
            // TODO : ICurrentPriceStorage ?
            TurtleBuyStrategy buyRunner = new TurtleBuyStrategy(memberStock, _baseData, true);

            await buyRunner.ExecuteAsync(token).ConfigureAwait(false);
        }

        private async Task SellOperationInternal(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IMemberBuyStock> memberBuyStocks = await _memoryDbOperations.GetMemberBuyStocksAsync(_dummyEmail).ConfigureAwait(false);

            if (memberBuyStocks == null || memberBuyStocks.Count == 0)
            {
                return;
            }

            IList<IMemberBuyStock> memberBuyStockList = memberBuyStocks.ToList();
            List<Task> tasks = new List<Task>();

            foreach (IMemberBuyStock memberBuyStock in memberBuyStockList)
            {
                if (memberBuyStocks.Count == 0)
                {
                    return;
                }

                // TODO : ICurrentPriceStorage ?
                TurtleSellStrategy sellStrategy = new TurtleSellStrategy(memberBuyStock, _baseData, true);

                Task t = sellStrategy.ExecuteAsync(token);
                tasks.Add(t);
            }

            try
            {
                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            }
            catch
            {
                //TODO : log ?
            }
        }
    }
}
