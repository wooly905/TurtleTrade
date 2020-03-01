using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure;

namespace TurtleTrade.ServiceWorkers.BuySellStrategy
{
    internal abstract class BuySellStrategyBase
    {
        protected IDatabaseOperations DatabaseOperations { get; }
        protected INofiticationService EmailService { get; }
        protected IBaseData BaseData { get; }
        protected IStockPriceNotificationChecker PriceNotificationChecker { get; }
        protected IEmailTemplateProvider EmailTemplateProvider { get; }
        private static readonly IDictionary<string, string> _stockNameDict;

        static BuySellStrategyBase()
        {
            _stockNameDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        protected BuySellStrategyBase(IBaseData baseData)
        {
            BaseData = baseData;
            DatabaseOperations = baseData.GetDatabaseOperations();
            EmailService = baseData.GetNotificationService();
            PriceNotificationChecker = baseData.CreateStockPriceNotificationChecker();
            EmailTemplateProvider = baseData.CreateEmailTemplateProvider();
        }

        protected async Task<bool> IfUserHoldStock(string memberEmail, string stockFullId)
        {
            IReadOnlyList<IMemberBuyStock> memberBuyStocks = await DatabaseOperations.GetMemberBuyStocksAsync(memberEmail).ConfigureAwait(false);

            return memberBuyStocks != null
                   && memberBuyStocks.Count(a => string.Equals(a.MemberEmail, memberEmail, StringComparison.OrdinalIgnoreCase)
                                                 && string.Equals(stockFullId, $"{a.Country.GetShortName()}.{a.StockId}", StringComparison.OrdinalIgnoreCase)) != 0;
        }

        protected async Task<decimal?> GetBreakThroughComparedPriceAsync(string stockFullId, BuySellStrategyType strategy)
        {
            IReadOnlyList<IAllPricesEntry> entries = await DatabaseOperations.GetTheLatestStockPriceAsync(BaseData.Country, BaseData.CurrentTime).ConfigureAwait(false);

            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            List<IAllPricesEntry> data2 = entries.Where(a => string.Equals($"{a.Country.GetShortName()}.{a.StockId}", stockFullId, StringComparison.OrdinalIgnoreCase)).ToList();

            if (data2.Count == 0)
            {
                return null;
            }

            // refactor!
            switch (strategy)
            {
                case BuySellStrategyType.N20:
                    return data2.FirstOrDefault()?.HighIn20;
                case BuySellStrategyType.N40:
                    return data2.FirstOrDefault()?.HighIn40;
                case BuySellStrategyType.N60:
                    return data2.FirstOrDefault()?.HighIn60;
                case BuySellStrategyType.MA20:
                    return data2.FirstOrDefault()?.MA20;
                case BuySellStrategyType.MA40:
                    return data2.FirstOrDefault()?.MA40;
                case BuySellStrategyType.MA60:
                    return data2.FirstOrDefault()?.MA60;
                case BuySellStrategyType.MA120:
                    return data2.FirstOrDefault()?.MA120;
                case BuySellStrategyType.MA240:
                    return data2.FirstOrDefault()?.MA240;
                case BuySellStrategyType.Unknown:
                    break;
            }

            return null;
        }

        protected async Task<decimal?> GetBreakDownComparedPriceAsync(string stockFullId, BuySellStrategyType strategy)
        {
            IReadOnlyList<IAllPricesEntry> entries = await DatabaseOperations.GetTheLatestStockPriceAsync(BaseData.Country, BaseData.CurrentTime).ConfigureAwait(false);

            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            List<IAllPricesEntry> data2 = entries.Where(a => string.Equals($"{a.Country.GetShortName()}.{a.StockId}", stockFullId, StringComparison.OrdinalIgnoreCase)).ToList();

            if (data2.Count == 0)
            {
                return null;
            }

            // TODO : refactor
            switch (strategy)
            {
                case BuySellStrategyType.N20:
                    return data2.FirstOrDefault()?.LowIn10;
                case BuySellStrategyType.N40:
                    return data2.FirstOrDefault()?.LowIn15;
                case BuySellStrategyType.N60:
                    return data2.FirstOrDefault()?.LowIn20;
                case BuySellStrategyType.MA20:
                    return data2.FirstOrDefault()?.MA20;
                case BuySellStrategyType.MA40:
                    return data2.FirstOrDefault()?.MA40;
                case BuySellStrategyType.MA60:
                    return data2.FirstOrDefault()?.MA60;
                case BuySellStrategyType.MA120:
                    return data2.FirstOrDefault()?.MA120;
                case BuySellStrategyType.MA240:
                    return data2.FirstOrDefault()?.MA240;
                case BuySellStrategyType.Unknown:
                    break;
            }

            return null;
        }

        

        protected async Task<string> GetStockNameAsync(string stockFullId)
        {
            _stockNameDict.TryGetValue(stockFullId, out string name);

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            IReadOnlyList<IStock> stocks = await DatabaseOperations.GetStocksAsync(BaseData.Country).ConfigureAwait(false);

            foreach (IStock stock in stocks)
            {
                string fullId = $"{stock.Country.GetShortName()}.{stock.StockId}";
                _stockNameDict[fullId] = stock.StockName;
            }

            _stockNameDict.TryGetValue(stockFullId, out name);

            return name;
        }

        protected async Task<IReadOnlyList<IMemberStock>> GetMovingAverageStrategyMemberStockListAsync()
        {
            IReadOnlyList<IMemberStock> memberStocks = await DatabaseOperations.GetMemberStocksAsync().ConfigureAwait(false);

            if (memberStocks == null || memberStocks.Count == 0)
            {
                return null;
            }

            return memberStocks.Where(a => (a.Strategy == BuySellStrategyType.MA20
                                           || a.Strategy == BuySellStrategyType.MA40
                                           || a.Strategy == BuySellStrategyType.MA60
                                           || a.Strategy == BuySellStrategyType.MA120
                                           || a.Strategy == BuySellStrategyType.MA240)
                                           && a.IsNotify).ToList();
        }

        protected async Task<IReadOnlyList<IMemberStock>> GetTurtleStrategyMemberStockListAsync(string memberEmail)
        {
            IReadOnlyList<IMemberStock> memberStocks = await DatabaseOperations.GetMemberStocksAsync(memberEmail).ConfigureAwait(false);

            if (memberStocks == null || memberStocks.Count == 0)
            {
                return null;
            }

            return memberStocks.Where(a => (a.Strategy == BuySellStrategyType.N20
                                           || a.Strategy == BuySellStrategyType.N40
                                           || a.Strategy == BuySellStrategyType.N60)
                                           && a.IsNotify).ToList();
        }
    }
}
