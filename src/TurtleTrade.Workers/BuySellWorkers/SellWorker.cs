using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;

namespace TurtleTrade.ServiceWorkers
{
    public class SellWorker : StockDataServiceWorker
    {
        private readonly IStockPriceNotificationChecker _priceNotifyChecker;
        private readonly IBuySellStrategyProvider _buySellStrategyProvider;

        public SellWorker(IBaseData baseData, IBuySellStrategyProvider runnerProvider)
            : base(baseData)
        {
            Name = "Sell-Worker";
            Kind = ServiceWorkerKind.SellWorker;
            SetWorkerStartTimeEndTime(ServiceWorkerKind.SellWorker);
            _buySellStrategyProvider = runnerProvider;
            _priceNotifyChecker = BaseData.CreateStockPriceNotificationChecker();

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

            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IMemberBuyStock> memberBuyStocks = await DatabaseOperations.GetMemberBuyStocksAsync().ConfigureAwait(false);

            if (memberBuyStocks == null || memberBuyStocks.Count == 0)
            {
                return;
            }

            memberBuyStocks = memberBuyStocks.Where(a => a.Country == BaseData.Country).ToList();
            List<Task> tasks = new List<Task>();

            foreach (IMemberBuyStock memberBuyStock in memberBuyStocks)
            {
                ITradingStrategy sellStrategy = _buySellStrategyProvider.GetSellStrategy(memberBuyStock, BaseData, TestStatus);
                Task t = sellStrategy.ExecuteAsync(token);
                tasks.Add(t);
            }

            try
            {
                // TODO , timeout?
                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            }
            catch
            {
                // TODO : log?
            }
        }
    }
}
