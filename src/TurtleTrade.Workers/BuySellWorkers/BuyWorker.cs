using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;

namespace TurtleTrade.ServiceWorkers
{
    public class BuyWorker : StockDataServiceWorker
    {
        private readonly IStockPriceNotificationChecker _priceNotificationChecker;
        private readonly IBuySellStrategyProvider _buySellStrategyProvider;

        public BuyWorker(IBaseData baseData, IBuySellStrategyProvider buySellStrategyProvider)
            : base(baseData)
        {
            Name = "Buy Stock Worker";
            _buySellStrategyProvider = buySellStrategyProvider;
            Kind = ServiceWorkerKind.BuyWorker;
            SetWorkerStartTimeEndTime(ServiceWorkerKind.BuyWorker);
            _priceNotificationChecker = BaseData.CreateStockPriceNotificationChecker();

            WriteToWorkerLog($"{Name} has been created");
        }

        protected override async Task RunInternalAsync(CancellationToken token)
        {
            WriteToHeartBeatLog();

            if (!BaseData.CurrentPriceStorage.IsAddedOrUpdated)
            {
                WriteToWorkerLog($"{Country} doesn't have updated current price storage.");
                return;
            }

            IReadOnlyList<IMemberStock> memberStocks = await DatabaseOperations.GetMemberStocksAsync().ConfigureAwait(false);

            if (memberStocks == null || memberStocks.Count == 0)
            {
                return;
            }

            List<Task> tasks = new List<Task>();

            foreach (IMemberStock memberStock in memberStocks)
            {
                if (!memberStock.IsNotify)
                {
                    continue;
                }

                ITradingStrategy strategy = _buySellStrategyProvider.GetBuyStrategy(memberStock, BaseData, TestStatus);

                if (strategy == null)
                {
                    continue;
                }

                Task t = strategy.ExecuteAsync(token);
                tasks.Add(t);
            }

            if (tasks.Count == 0)
            {
                return;
            }

            try
            {
                // TODO :timeout ?
                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            }
            catch
            {
                // TODO: log?
            }
        }
    }
}
