using System;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure;

namespace TurtleTrade.ServiceWorkers.BuySellStrategy
{
    internal class MovingAverageBuyStrategy : BuySellStrategyBase, ITradingStrategy
    {
        private readonly bool _isBackTest;
        private readonly bool _testStatus;

        public MovingAverageBuyStrategy(IBaseData baseData, bool isBackTest = false, bool testStatus = false)
            : base(baseData)
        {
            _isBackTest = isBackTest;
            _testStatus = testStatus;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            foreach (IMemberStock memberStock in await GetMovingAverageStrategyMemberStockListAsync().ConfigureAwait(false))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    _ = MovingAverageBuyInternalAsync(BaseData.Country, memberStock, BaseData.CurrentPriceStorage);
                }
                catch (Exception ex)
                {
                    _ = BaseData.GetLogger().WriteToErrorLogAsync(BaseData.Country,
                                                                       BaseData.CurrentTime,
                                                                       "MovingAverageBuyStrategy",
                                                                       ex);
                }
            }
        }

        private async Task MovingAverageBuyInternalAsync(CountryKind country, IMemberStock memberStock, ICurrentPriceStorage storage)
        {
            string stockFullId = $"{country.GetShortName()}.{memberStock.StockId}";

            if (!storage.TryGetItem(country, memberStock.StockId, out ICurrentPrice target)
                || target == null
                || !BaseData.CurrentTime.IsSameDay(target.LastTradeTime))
            {
                return;
            }

            decimal todayHigh = target.TodayHighPrice;
            decimal? previousMovingAveragePrice = await GetBreakThroughComparedPriceAsync(stockFullId, memberStock.Strategy).ConfigureAwait(false);

            if (!previousMovingAveragePrice.HasValue || previousMovingAveragePrice.Value >= todayHigh)
            {
                return;
            }

            bool isUserHoldStock = await IfUserHoldStock(memberStock.MemberEmail, stockFullId).ConfigureAwait(false);

            if (isUserHoldStock)
            {
                return;
            }

            // TODO : refactor
            // check if notify today  
            if (!PriceNotificationChecker.CanNotify(memberStock.MemberEmail,
                                                    stockFullId,
                                                    StockNotificationType.Buy,
                                                    BaseData.CurrentTime,
                                                    memberStock.Strategy,
                                                    StockBuyState.Buy))
            {
                return;
            }

            string stockName = await GetStockNameAsync(stockFullId).ConfigureAwait(false);
            // should change to some EmailTemplateProvider
            IEmailTemplate emailMessage = EmailTemplateProvider.GetBreakThroughEmailTemplate(memberStock.MemberEmail,
                                                                                             stockFullId,
                                                                                             stockName,
                                                                                             memberStock.Strategy,
                                                                                             todayHigh,
                                                                                             previousMovingAveragePrice.HasValue ? previousMovingAveragePrice.Value : -1);

            if (_testStatus)
            {
                await EmailService.SendEmailAsync(country, BaseData.CurrentTime, emailMessage).ConfigureAwait(false);
            }
            else
            {
                await EmailService.SendEmailAsync(country, BaseData.CurrentTime, emailMessage).ConfigureAwait(false);
            }


            PriceNotificationChecker.InsertRecord(memberStock.MemberEmail, stockFullId, StockNotificationType.Buy, BaseData.CurrentTime, memberStock.Strategy, StockBuyState.Buy);

            if (!_testStatus)
            {
                _ = BaseData.GetLogger().WriteToWorkerLogAsync(country, BaseData.CurrentTime, "MovingAverageBuyStrategy", $"{memberStock.Strategy}, first buy ({stockFullId}) email is sent to {memberStock.MemberEmail}");
            }

            if (_isBackTest)
            {
                //await BackTestFirstBuyOperationAsync(previousHighBoundPrice.Value);
            }
        }

        //private async Task BackTestMovingAverageBuyAsync(IMemberStock memberStock, decimal buyPrice)
        //{
        //    IReadOnlyList<IStockPriceHistory> items = await DatabaseOperations.GetStockPriceHistoryAsync(memberStock.Country, memberStock.StockId, BaseData.CurrentTime, 1).ConfigureAwait(false);

        //    if (items == null || items.Count == 0)
        //    {
        //        //_writeToErrorLogAction(new Exception($"BuyStockRunner: No stock price history data on previous business day of {BaseData.CurrentTime:yyyy-MM-dd}"));
        //        return;
        //    }

        //    IStockPriceHistory item = items.FirstOrDefault();

        //    if (item == null)
        //    {
        //        return;
        //    }

        //    decimal N = -1m;

        //    // TODO : refactor
        //    switch (memberStock.Strategy)
        //    {
        //        case BuySellStrategyType.MA20:
        //            if (item.MA20 != null)
        //            {
        //                N = item.MA20.Value;
        //            }

        //            break;
        //        case BuySellStrategyType.MA40:
        //            if (item.MA40 != null)
        //            {
        //                N = item.MA40.Value;
        //            }

        //            break;
        //        case BuySellStrategyType.MA60:
        //            if (item.MA60 != null)
        //            {
        //                N = item.MA60.Value;
        //            }

        //            break;
        //        case BuySellStrategyType.MA120:
        //            if (item.MA120 != null)
        //            {
        //                N = item.MA120.Value;
        //            }

        //            break;
        //        case BuySellStrategyType.MA240:
        //            if (item.MA240 != null)
        //            {
        //                N = item.MA240.Value;
        //            }

        //            break;
        //    }

        //    if (N == -1m)
        //    {
        //        return;
        //    }

        //    await DatabaseOperations.AddMemberBuyStockAsync(memberStock.MemberEmail, memberStock.Country, memberStock.StockId, buyPrice, N, memberStock.Strategy, BaseData.CurrentTime).ConfigureAwait(false);
        //}
    }
}
